using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.Dropdowns;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.Search;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Scheduling;
using Wobble.Window;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel
{
    public class SelectFilterPanel : Sprite
    {
        /// <summary>
        ///     The mapsets that are currently available to play
        /// </summary>
        private Bindable<List<Mapset>> AvailableMapsets { get; }

        /// <summary>
        ///     The current search the user has had
        /// </summary>
        private Bindable<string> CurrentSearchQuery { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> IsPlayTesting { get; }

        /// <summary>
        /// </summary>
        private Bindable<SelectContainerPanel> ActiveLeftPanel { get; }

        /// <summary>
        ///     Underlying button that prevents mapsets from being clicked from inside the area
        /// </summary>
        private ImageButton Button { get; }

        /// <summary>
        ///     The banner that displays the current map's background
        /// </summary>
        private FilterPanelBanner Banner { get; }

        /// <summary>
        ///     Displays the current map information
        /// </summary>
        private FilterPanelMapInfo MapInfo { get; }

        /// <summary>
        ///     Items that are aligned from right to left
        /// </summary>
        private List<Drawable> RightItems { get; }

        /// <summary>
        ///     The textbox to search for maps
        /// </summary>
        public FilterPanelSearchBox SearchBox { get; private set; }

        /// <summary>
        ///     The text that displays how many maps are available
        /// </summary>
        private FilterPanelMapsAvailable MapsAvailable { get; set; }

        /// <summary>
        ///     The dropdown to sort maps
        /// </summary>
        private FilterDropdownSorting SortDropdown { get; set; }

        /// <summary>
        ///     The dropdown to group maps
        /// </summary>
        private FilterDropdownGroupBy SortGroupBy { get; set; }

        /// <summary>
        ///     The dropdown to sort by mode
        /// </summary>
        private FilterDropdownMode SortMode { get; set; }

        /// <summary>
        ///     Task used to filter mapsets
        /// </summary>
        private TaskHandler<int, int> FilterMapsetsTask { get; }

        /// <summary>
        /// </summary>
        public SelectFilterPanel(Bindable<List<Mapset>> availableMapsets, Bindable<string> currentSearchQuery,
            Bindable<bool> isPlayTesting, Bindable<SelectContainerPanel> activeLeftPanel)
        {
            AvailableMapsets = availableMapsets;
            CurrentSearchQuery = currentSearchQuery;
            IsPlayTesting = isPlayTesting;
            ActiveLeftPanel = activeLeftPanel;

            Size = new ScalableVector2(WindowManager.Width, 88);
            Tint = ColorHelper.HexToColor("#242424");

            Banner = new FilterPanelBanner(this)
            {
                Parent = this,
                Alignment = Alignment.MidLeft
            };

            MapInfo = new FilterPanelMapInfo
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 25
            };

            RightItems = new List<Drawable>();

            CreateSortDropdown();
            CreateSortGroupBy();
            CreateSortModeDropdown();
            CreateSearchBox();
            CreateMapsAvailable();

            FilterMapsetsTask = new TaskHandler<int, int>(StartFilterMapsetsTask);

            CurrentSearchQuery.ValueChanged += (sender, args) => StartFilterMapsetsTask();

            if (ConfigManager.SelectOrderMapsetsBy != null)
                ConfigManager.SelectOrderMapsetsBy.ValueChanged += OnSelectOrderMapsetsChanged;

            if (ConfigManager.SelectFilterGameModeBy != null)
                ConfigManager.SelectFilterGameModeBy.ValueChanged += OnSelectFilterGameModeChanged;

            if (ConfigManager.SelectGroupMapsetsBy != null)
                ConfigManager.SelectGroupMapsetsBy.ValueChanged += OnSelectGroupMapsetsChanged;

            MapManager.Selected.ValueChanged += OnMapChanged;
            ModManager.ModsChanged += OnModsChanged;
            PlaylistManager.Selected.ValueChanged += OnPlaylistChanged;
            PlaylistManager.PlaylistMapsManaged += OnPlaylistMapsManaged;

            AlignRightItems();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable twice DelegateSubtraction
            if (ConfigManager.SelectOrderMapsetsBy != null)
                ConfigManager.SelectOrderMapsetsBy.ValueChanged -= OnSelectOrderMapsetsChanged;

            if (ConfigManager.SelectFilterGameModeBy != null)
                ConfigManager.SelectFilterGameModeBy.ValueChanged -= OnSelectFilterGameModeChanged;

            if (ConfigManager.SelectGroupMapsetsBy != null)
                ConfigManager.SelectGroupMapsetsBy.ValueChanged -= OnSelectGroupMapsetsChanged;

            MapManager.Selected.ValueChanged -= OnMapChanged;
            ModManager.ModsChanged -= OnModsChanged;
            PlaylistManager.Selected.ValueChanged -= OnPlaylistChanged;
            PlaylistManager.PlaylistMapsManaged -= OnPlaylistMapsManaged;

            FilterMapsetsTask?.Dispose();

            base.Destroy();
        }

        /// <summary>
        ///     Creates <see cref="SortDropdown"/>
        /// </summary>
        private void CreateSortDropdown()
        {
            SortDropdown = new FilterDropdownSorting(AvailableMapsets) { Parent = this, };
            RightItems.Add(SortDropdown);
        }

        /// <summary>
        ///     Creates <see cref="SortGroupBy"/>
        /// </summary>
        private void CreateSortGroupBy()
        {
            SortGroupBy = new FilterDropdownGroupBy(AvailableMapsets) { Parent = this };
            RightItems.Add(SortGroupBy);
        }

        /// <summary>
        ///     Creates <see cref="SortMode"/>
        /// </summary>
        private void CreateSortModeDropdown()
        {
            SortMode = new FilterDropdownMode(AvailableMapsets) { Parent = this };
            RightItems.Add(SortMode);
        }

        /// <summary>
        ///     Creates <see cref="MapsAvailable"/>
        /// </summary>
        private void CreateMapsAvailable()
        {
            MapsAvailable = new FilterPanelMapsAvailable(AvailableMapsets) { Parent = this };
            RightItems.Add(MapsAvailable);
        }

        /// <summary>
        ///     Creates <see cref="SearchBox"/>
        /// </summary>
        private void CreateSearchBox()
        {
            SearchBox = new FilterPanelSearchBox(CurrentSearchQuery, AvailableMapsets, IsPlayTesting, ActiveLeftPanel,
                    "Type to search...")
            {
                Parent = this
            };

            RightItems.Add(SearchBox);
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
                var spacing = 30;

                if (i == 0)
                    item.X = -padding;
                else
                    item.X = RightItems[i - 1].X - RightItems[i - 1].Width - spacing;
            }
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
            lock (AvailableMapsets.Value)
            {
                Logger.Important($"Filtering mapsets by -  Query: `{CurrentSearchQuery.Value}` | Sort By: {ConfigManager.SelectOrderMapsetsBy?.Value}",
                    LogType.Runtime, false);

                AvailableMapsets.Value = MapsetHelper.FilterMapsets(CurrentSearchQuery);

                if (AvailableMapsets.Value.Count == 0)
                    return;

                // Check if the map is in any of the mapsets
                if (MapManager.Selected.Value != null)
                {
                    foreach (var set in AvailableMapsets.Value)
                    {
                        if (set.Maps.Any(x => x.Md5Checksum == MapManager.Selected.Value.Md5Checksum))
                            return;
                    }
                }

                MapManager.SelectMapFromMapset(AvailableMapsets.Value.First());
                BackgroundHelper.Load(MapManager.Selected.Value);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectOrderMapsetsChanged(object sender, BindableValueChangedEventArgs<OrderMapsetsBy> e) => StartFilterMapsetsTask();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectFilterGameModeChanged(object sender, BindableValueChangedEventArgs<SelectFilterGameMode> e) => StartFilterMapsetsTask();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectGroupMapsetsChanged(object sender, BindableValueChangedEventArgs<GroupMapsetsBy> e) => StartFilterMapsetsTask();

        /// <summary>
        ///     Responsible for initiating the new banner load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            ThreadScheduler.Run(() =>
            {
                lock (Banner)
                lock (Banner.Background.Image)
                {
                    // This has to be multi-threaded because MapManager.GetBackgroundPath
                    // parses osu! maps to get the path of BG. It doesn't exist in the osu!db
                    if (MapManager.GetBackgroundPath(e.OldValue) == MapManager.GetBackgroundPath(e.Value))
                        return;

                    Banner.FadeToBlack();
                    BackgroundHelper.Load(e.Value);
                }
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnModsChanged(object sender, ModsChangedEventArgs e)
        {
            if (ConfigManager.SelectOrderMapsetsBy == null)
                return;

            if (ConfigManager.SelectOrderMapsetsBy.Value != OrderMapsetsBy.Difficulty)
                return;

            var isSpeedMod = e.ChangedMods >= ModIdentifier.Speed05X && e.ChangedMods <= ModIdentifier.Speed20X ||
                             e.ChangedMods >= ModIdentifier.Speed055X && e.ChangedMods <= ModIdentifier.Speed095X ||
                             e.ChangedMods >= ModIdentifier.Speed105X && e.ChangedMods <= ModIdentifier.Speed195X;

            if (e.Type == ModChangeType.RemoveAll || e.Type == ModChangeType.RemoveSpeed || isSpeedMod)
                StartFilterMapsetsTask();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlaylistChanged(object sender, BindableValueChangedEventArgs<Playlist> e) => StartFilterMapsetsTask();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlaylistMapsManaged(object sender, PlaylistMapsManagedEventArgs e)
        {
            if (ConfigManager.SelectGroupMapsetsBy.Value != GroupMapsetsBy.Playlists)
                return;

            StartFilterMapsetsTask();
        }
    }
}
