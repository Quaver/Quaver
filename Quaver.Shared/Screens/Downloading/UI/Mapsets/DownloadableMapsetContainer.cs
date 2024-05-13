using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Helpers;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online.API.MapsetSearch;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Scheduling;
using Wobble.Window;

namespace Quaver.Shared.Screens.Downloading.UI.Mapsets
{
    public class DownloadableMapsetContainer : SongSelectContainer<DownloadableMapset>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override SelectScrollContainerType Type { get; }

        /// <summary>
        /// </summary>
        private BindableList<DownloadableMapset> AvailableMapsets { get; }

        /// <summary>
        /// </summary>
        private Bindable<DownloadableMapset> SelectedMapset { get; }

        /// <summary>
        /// </summary>
        private Bindable<int> Page { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> ReachedEnd { get; }

        /// <summary>
        /// </summary>
        private TaskHandler<int, int> SearchTask { get; }

        /// <summary>
        /// </summary>
        private LoadingWheel LoadingWheel { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="mapsets"></param>
        /// <param name="selectedMapset"></param>
        /// <param name="page"></param>
        /// <param name="searchTask"></param>
        public DownloadableMapsetContainer(BindableList<DownloadableMapset> mapsets,
            Bindable<DownloadableMapset> selectedMapset, Bindable<int> page, Bindable<bool> reachedEnd, TaskHandler<int, int> searchTask)
            : base(mapsets.Value, int.MaxValue)
        {
            AvailableMapsets = mapsets;
            SelectedMapset = selectedMapset;
            Page = page;
            ReachedEnd = reachedEnd;
            SearchTask = searchTask;

            CreateLoadingWheel();

            AvailableMapsets.ItemRemoved += OnAvailableItemRemoved;
            AvailableMapsets.ValueChanged += OnAvailableMapsetChanged;
            AvailableMapsets.MultipleItemsAdded += OnMultipleItemsAdded;
            MapsetDownloadManager.DownloadAdded += OnDowloadAdded;
            SelectedMapset.ValueChanged += OnSelectedMapsetChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Handle infinite scrolling
            if (ContentContainer.Height - Math.Abs(ContentContainer.Y) - Height < 500 && !SearchTask.IsRunning
                && !ReachedEnd.Value)
            {
                Page.Value++;
            }

            var alpha = (Page.Value == 0 || AvailableMapsets.Value.Count == 0) && SearchTask.IsRunning ? 1 : 0;

            LoadingWheel.Alpha = MathHelper.Lerp(LoadingWheel.Alpha, alpha,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 30, 1));

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            MapsetDownloadManager.DownloadAdded -= OnDowloadAdded;
            // ReSharper disable once DelegateSubtraction
            SelectedMapset.ValueChanged -= OnSelectedMapsetChanged;

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void HandleInput(GameTime gameTime)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<DownloadableMapset> CreateObject(DownloadableMapset item, int index)
            => new DrawableDownloadableMapset(this, item, index, SelectedMapset);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override float GetSelectedPosition() => (-SelectedIndex.Value + 4) * DrawableMapset.MapsetHeight;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void SetSelectedIndex()
        {
        }

        /// <summary>
        ///     Initializes the container with new maps
        /// </summary>
        /// <param name="maps"></param>
        public void Initialize(List<DownloadableMapset> maps)
        {
            DestroyAndClearPool();

            AvailableItems = maps;

            // Get the new selected mapset index
            SetSelectedIndex();

            // Reset the starting index so we can be aware of the mapsets that are needed
            PoolStartingIndex = DesiredPoolStartingIndex(SelectedIndex.Value);

            // Recreate the object pool
            CreatePool(false);

            if (maps == null || maps.Count == 0)
                ContentContainer.Height = Height;

            PositionAndContainPoolObjects();

            ContentContainer.Animations.Clear();
            ContentContainer.Y = 0;
            PreviousContentContainerY = ContentContainer.Y;
            TargetY = PreviousContentContainerY;
            PreviousTargetY = PreviousContentContainerY;
        }

        /// <summary>
        /// </summary>
        private void CreateLoadingWheel() => LoadingWheel = new LoadingWheel
        {
            Parent = this,
            Size = new ScalableVector2(50, 50),
            Alignment = Alignment.MidCenter,
            Alpha = 0
        };

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAvailableMapsetChanged(object sender, BindableValueChangedEventArgs<List<DownloadableMapset>> e)
            => ScheduleUpdate(() => Initialize(e.Value));

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDowloadAdded(object sender, MapsetDownloadAddedEventArgs e)
        {
            e.Download.Status.ValueChanged += (o, args) =>
            {
                if (args.Value.Status != FileDownloaderStatus.Complete)
                    return;
                if (args.Value.Error == null)
                {
                    NotificationManager.Show(NotificationLevel.Success,
                        $"Finished downloading: {e.Download.Artist} - {e.Download.Title}!");
                }

                var index = AvailableMapsets.Value.FindIndex(x => x.Id == e.Download.MapsetId);

                var availableItem = AvailableMapsets.Value.Find(x => x.Id == e.Download.MapsetId);

                if (availableItem != null)
                    AvailableMapsets.Remove(availableItem);

                if (SelectedMapset.Value.Id != e.Download.MapsetId)
                    return;

                /*// Select the next mapset if the selected one was downloaded
                if (index <= 0 && AvailableMapsets.Value.Count != 0)
                    SelectedMapset.Value = AvailableMapsets.Value.First();
                else if (index > 0 && index < AvailableMapsets.Value.Count)
                    SelectedMapset.Value = AvailableMapsets.Value[index];*/
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAvailableItemRemoved(object sender, BindableListItemRemovedEventArgs<DownloadableMapset> e)
        {
            AddScheduledUpdate(() =>
            {
                var item = Pool.Find(x => x.Item.Id == e.Item.Id);

                if (item == null)
                    return;

                Pool.Remove(item);
                item.Destroy();

                for (var i = 0; i < Pool.Count; i++)
                {
                    Pool[i].Index = i;

                    Pool[i].ClearAnimations();
                    Pool[i].MoveToY((PoolStartingIndex + i) * Pool[i].HEIGHT + PaddingTop, Easing.OutQuint, 7500);
                }

                RecalculateContainerHeight();
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMultipleItemsAdded(object sender, BindableListMultipleItemsAddedEventArgs<DownloadableMapset> e)
        {
            AddScheduledUpdate(() =>
            {
                foreach (var item in e.Items)
                {
                    var mapset = new DrawableDownloadableMapset(this, item, Pool.Count, SelectedMapset);
                    mapset.UpdateContent(mapset.Item, mapset.Index);

                    Pool.Add(mapset);
                    AddContainedDrawable(mapset);
                    mapset.Y = (PoolStartingIndex + mapset.Index) * Pool[mapset.Index].HEIGHT + PaddingTop;
                }

                RecalculateContainerHeight();
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedMapsetChanged(object sender, BindableValueChangedEventArgs<DownloadableMapset> e)
        {
            SelectedIndex.Value = AvailableMapsets.Value.IndexOf(e.Value);
            ScrollToSelected(3000);
        }
    }
}
