using System.Collections.Generic;
using System.Threading;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Music.UI.Controller.Search.Dropdowns;
using Quaver.Shared.Screens.Selection.UI.FilterPanel;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.Search;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Logging;
using Wobble.Scheduling;

namespace Quaver.Shared.Screens.Music.UI.Controller.Search
{
    public class MusicControllerSearchPanel : Sprite
    {
        /// <summary>
        /// </summary>
        public Bindable<string> CurrentSearchQuery { get; }

        /// <summary>
        /// </summary>
        public Bindable<List<Mapset>> AvailableSongs { get; }

        /// <summary>
        ///     Items that are aligned from right to left
        /// </summary>
        private List<Drawable> RightItems { get; }
        
        /// <summary>
        /// </summary>
        private MusicControllerSortDropdown SortDropdown { get; set; }

        /// <summary>
        /// </summary>
        private MusicControllerPlaylistDropdown PlaylistDropdown { get; set; }

        /// <summary>
        /// </summary>
        private FilterPanelSearchBox SearchBox { get; set; }

        /// <summary>
        /// </summary>
        private FilterPanelMapsAvailable SongsFound { get; set; }

        /// <summary>
        /// </summary>
        private TaskHandler<int, int> FilterMapsetsTask { get; }

        /// <summary>
        /// </summary>
        public MusicControllerSearchPanel(float width, Bindable<string> currentSearchQuery, Bindable<List<Mapset>> availableSongs)
        {
            CurrentSearchQuery = currentSearchQuery;
            AvailableSongs = availableSongs;

            Size = new ScalableVector2(width, 86);
            Tint = ColorHelper.HexToColor("#1f1f1f");

            RightItems = new List<Drawable>();

            CreateSortDropdown();
            CreatePlaylistDropdown();
            CreateSearchBox();
            CreateSongsFound();

            AlignRightItems();

            FilterMapsetsTask = new TaskHandler<int, int>(StartFilterMapsetsTask);
            CurrentSearchQuery.ValueChanged += (sender, args) => StartFilterMapsetsTask();

            if (ConfigManager.MusicPlayerOrderMapsBy != null)
                ConfigManager.MusicPlayerOrderMapsBy.ValueChanged += OnOrderMapsByChanged;

            PlaylistManager.Selected.ValueChanged += OnPlaylistChanged;

            FilterMapsets();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            if (ConfigManager.MusicPlayerOrderMapsBy != null)
                ConfigManager.MusicPlayerOrderMapsBy.ValueChanged -= OnOrderMapsByChanged;

            // ReSharper disable once DelegateSubtraction
            PlaylistManager.Selected.ValueChanged -= OnPlaylistChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Aligns the items from right to left
        /// </summary>
        private void AlignRightItems()
        {
            for (var i = 0; i < RightItems.Count; i++)
            {
                var item = RightItems[i];

                item.Parent = this;

                item.Alignment = Alignment.MidRight;

                const int padding = 25;
                var spacing = 50;

                if (i == 0)
                    item.X = -padding;
                else
                    item.X = RightItems[i - 1].X - RightItems[i - 1].Width - spacing;
            }
        }
        
        /// <summary>
        /// </summary>
        private void CreateSortDropdown()
        {
            SortDropdown = new MusicControllerSortDropdown
            {
                Parent = this,
                Alignment = Alignment.MidRight
            };

            RightItems.Add(SortDropdown);
        }

        /// <summary>
        /// </summary>
        private void CreatePlaylistDropdown()
        {
            PlaylistDropdown = new MusicControllerPlaylistDropdown()
            {
                Parent = this,
                Alignment = Alignment.MidRight
            };

            RightItems.Add(PlaylistDropdown);
        }

        /// <summary>
        /// </summary>
        private void CreateSearchBox()
        {
            SearchBox = new FilterPanelSearchBox(CurrentSearchQuery, AvailableSongs, new Bindable<bool>(false), null,
                "Type to search...")
            {
                Parent = this,
                Alignment = Alignment.MidRight,
            };

            SearchBox.Width -= 20;
            RightItems.Add(SearchBox);
        }

        /// <summary>
        /// </summary>
        private void CreateSongsFound()
        {
            SongsFound = new FilterPanelMapsAvailable(AvailableSongs, true)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = 25
            };

            RightItems.Add(SongsFound);
        }

        /// <summary>
        /// </summary>
        private void StartFilterMapsetsTask()
        {
            if (FilterMapsetsTask.IsRunning)
                FilterMapsetsTask.Cancel();

            FilterMapsetsTask.Run(0);
        }

        /// <summary>
        ///     Begins the task to filter mapsets
        /// </summary>
        /// <param name="a"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private int StartFilterMapsetsTask(int a, CancellationToken token)
        {
            FilterMapsets();
            return 0;
        }

        /// <summary>
        ///     Handles filtering mapsets for the screen
        /// </summary>
        private void FilterMapsets()
        {
            lock (AvailableSongs.Value)
            {
                Logger.Important($"Filtering mapsets by -  Query: `{CurrentSearchQuery.Value}`", LogType.Runtime, false);
                AvailableSongs.Value = MapsetHelper.FilterMapsets(CurrentSearchQuery, true);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOrderMapsByChanged(object sender, BindableValueChangedEventArgs<OrderMapsetsBy> e)
        {
            // Here we trigger a search query change to handle the loading wheel animation as well
            // as re-filtering the songs in the handled event
            CurrentSearchQuery.TriggerChange();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlaylistChanged(object sender, BindableValueChangedEventArgs<Playlist> e) => CurrentSearchQuery.TriggerChange();
    }
}