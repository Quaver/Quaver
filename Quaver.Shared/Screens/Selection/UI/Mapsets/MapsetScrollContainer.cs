using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Scheduling;
using TagLib.Ape;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Scheduling;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets
{
    public class MapsetScrollContainer : SongSelectContainer<Mapset>
    {
        public override SelectScrollContainerType Type { get; } = SelectScrollContainerType.Mapsets;

        /// <summary>
        /// </summary>
        private Bindable<List<Mapset>> AvailableMapsets { get; }

        /// <summary>
        ///     The currently active container in song select
        /// </summary>
        public Bindable<SelectScrollContainerType> ActiveScrollContainer { get; }

        /// <summary>
        ///     The amount of time that has elapsed since the user requested to initialize the mapsets
        /// </summary>
        private double TimeElapsedUntilInitializationRequest { get; set; } = ReinitializeTime;

        /// <summary>
        ///     The time it takes until the mapsets will reinitialize
        /// </summary>
        private const int ReinitializeTime = 250;

        /// <summary>
        ///     If the mapsets have reinitialized
        /// </summary>
        private bool HasReinitialized { get; set; }

        /// <summary>
        ///     The mapset that is in the middle of the screen
        /// </summary>
        private DrawableMapset MiddleMapset
        {
            get
            {
                if (Pool.Count == 0)
                    return null;

                return Pool[Pool.Count / 2] as DrawableMapset;
            }
        }

        /// <summary>
        ///     Whether frequently changing text is being cached or not
        /// </summary>
        private bool _isCached = true;
        public bool IsCached
        {
            get => _isCached;
            private set
            {
                if (value == _isCached)
                    return;

                SetCaching(value);
                _isCached = value;
            }
        }

        /// <summary>
        ///     Caching is disabled when the scroll speed exceeds this value.
        ///     This value should be low enough to disable caching when selecting a random mapset, absolute scrolling, mashing PgUp/PgDn,
        ///     but should be high enough to avoid disabling caching when up/down are mashed.
        ///
        ///     Units: change in y-position / milliseconds since last update
        /// </summary>
        public double CacheThreshold { get; } = 100;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="availableMapsets"></param>
        /// <param name="activeScrollContainer"></param>
        public MapsetScrollContainer(Bindable<List<Mapset>> availableMapsets, Bindable<SelectScrollContainerType> activeScrollContainer)
            : base(availableMapsets.Value, 12)
        {
            AvailableMapsets = availableMapsets;
            ActiveScrollContainer = activeScrollContainer;

            MapManager.Selected.ValueChanged += OnMapChanged;
            MapManager.MapsetDeleted += OnMapsetDeleted;
            AvailableMapsets.ValueChanged += OnAvailableMapsetsChanged;
            SelectionScreen.RandomMapsetSelected += OnRandomMapsetSelected;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            TimeElapsedUntilInitializationRequest += gameTime.ElapsedGameTime.TotalMilliseconds;
            InitializeMapsets(false);

            // Disable caching of frequently changing text if scrolling fast enough
            // Caching stays disabled until scrolling stops to prevent repeated recaching when scroll speed fluctuates
            var deltaY = CurrentY - PreviousY;
            var speed = Math.Abs(deltaY) / gameTime.ElapsedGameTime.TotalMilliseconds;
            IsCached = !(speed > CacheThreshold || (!IsCached && speed != 0));

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable twice DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;
            MapManager.MapsetDeleted -= OnMapsetDeleted;
            AvailableMapsets.ValueChanged -= OnAvailableMapsetsChanged;
            SelectionScreen.RandomMapsetSelected -= OnRandomMapsetSelected;

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override float GetSelectedPosition() => (-SelectedIndex.Value + 4) * DrawableMapset.MapsetHeight;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<Mapset> CreateObject(Mapset item, int index) => new DrawableMapset(this, item, index);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void HandleInput(GameTime gameTime)
        {
            if (ActiveScrollContainer.Value != SelectScrollContainerType.Mapsets)
                return;

            // Pressing up and down while the current mapset is not visible to scroll to the one
            // that is in the middle
            if ((KeyboardManager.IsUniqueKeyPress(Keys.Left) || KeyboardManager.IsUniqueKeyPress(Keys.Right)) && CanScrollToMiddleMapset())
            {
                MapManager.SelectMapFromMapset(MiddleMapset.Item);
                SelectedIndex.Value = AvailableMapsets.Value.IndexOf(MiddleMapset.Item);

                ScrollToSelected();
            }
            // Move to the next mapset
            else if (KeyboardManager.IsUniqueKeyPress(Keys.Right) || KeyboardManager.IsUniqueKeyPress(Keys.Down))
            {
                if (SelectedIndex.Value + 1 >= AvailableMapsets.Value.Count)
                    return;

                MapManager.SelectMapFromMapset(AvailableMapsets.Value[SelectedIndex.Value + 1]);
                SelectedIndex.Value++;

                ScrollToSelected();
            }
            // Move to the previous mapset
            else if (KeyboardManager.IsUniqueKeyPress(Keys.Left) || KeyboardManager.IsUniqueKeyPress(Keys.Up))
            {
                if (SelectedIndex.Value - 1 < 0)
                    return;

                MapManager.SelectMapFromMapset(AvailableMapsets.Value[SelectedIndex.Value - 1]);

                SelectedIndex.Value--;
                ScrollToSelected();
            }
            // Move to the next difficulty of a mapset
            else if ((KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
                && KeyboardManager.IsUniqueKeyPress(Keys.PageDown))
            {
                InputEnabled = false;

                if (!MapsetHelper.IsSingleDifficultySorted())
                    return;

                var val = SelectedIndex.Value;

                for (var i = val; i != val - 1; i++)
                {
                    if (i == AvailableMapsets.Value.Count)
                        i = 0;

                    var mapset = AvailableMapsets.Value[i];

                    if (AvailableMapsets.Value[i].Maps.First().Mapset != MapManager.Selected.Value.Mapset)
                        continue;

                    if (mapset.Maps.First() == MapManager.Selected.Value)
                        continue;

                    SelectedIndex.Value = AvailableMapsets.Value.IndexOf(mapset);
                    MapManager.SelectMapFromMapset(AvailableMapsets.Value[SelectedIndex.Value]);

                    ScrollToSelected();
                    break;
                }
            }
            // Move to the previous difficulty of a mapset
            else if ((KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
                && KeyboardManager.IsUniqueKeyPress(Keys.PageUp))
            {
                InputEnabled = false;

                if (!MapsetHelper.IsSingleDifficultySorted())
                    return;

                var val = SelectedIndex.Value;

                for (var i = val; i != val + 1; i--)
                {
                    if (i == -1)
                        i = AvailableMapsets.Value.Count - 1;

                    var mapset = AvailableMapsets.Value[i];

                    if (AvailableMapsets.Value[i].Maps.First().Mapset != MapManager.Selected.Value.Mapset)
                        continue;

                    if (mapset.Maps.First() == MapManager.Selected.Value)
                        continue;

                    SelectedIndex.Value = AvailableMapsets.Value.IndexOf(mapset);
                    MapManager.SelectMapFromMapset(AvailableMapsets.Value[SelectedIndex.Value]);

                    ScrollToSelected();
                    break;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private void InitializeMapsets(bool restart)
        {
            if (restart)
            {
                TimeElapsedUntilInitializationRequest = 0;
                HasReinitialized = false;
                return;
            }

            if (TimeElapsedUntilInitializationRequest < ReinitializeTime || HasReinitialized)
                return;

            lock (Pool)
            {
                DestroyAndClearPool();

                AvailableItems = AvailableMapsets.Value;

                SetSelectedIndex();

                // Reset the starting index so we can be aware of the mapsets that are needed
                PoolStartingIndex = DesiredPoolStartingIndex(SelectedIndex.Value);

                // Recreate the object pool
                CreatePool();
                PositionAndContainPoolObjects();

                // Snap to it immediately
                SnapToSelected();
            }

            HasReinitialized = true;
            FireInitializedEvent();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => ScrollToSelected();

        /// <summary>
        ///     Called when the list of available maps has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAvailableMapsetsChanged(object sender, BindableValueChangedEventArgs<List<Mapset>> e) => InitializeMapsets(true);

        /// <summary>
        ///     Properly sets the selected index when a random map was selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRandomMapsetSelected(object sender, RandomMapsetSelectedEventArgs e)
        {
            SelectedIndex.Value = e.Index;
            ScrollToSelected(800);
        }

        /// <summary>
        ///     Sets the appropriate index of the selected mapset
        /// </summary>
        protected override void SetSelectedIndex()
        {
            SelectedIndex.Value = AvailableItems.FindIndex(x => x.Maps.Contains(MapManager.Selected.Value));

            var oldValue = SelectedIndex.Value;

            if (SelectedIndex.Value == -1)
                SelectedIndex.Value = 0;

            // Manually trigger the change event in the case that the selected index is still the same value.
            // Other components that rely on the bindable will want to update their state in case
            // the mapset has changed in any way.
            if (oldValue == SelectedIndex.Value)
                SelectedIndex.TriggerChangeEvent();
        }

        /// <summary>
        ///     When a mapset has been deleted, reset the selected index of the container to the newly selected mapset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapsetDeleted(object sender, MapsetDeletedEventArgs e)
        {
            if (e.Index == -1)
                SelectedIndex.Value = 0;

            if (e.Index - 1 < 0)
                return;

            SelectedIndex.Value = e.Index - 1;
        }

        /// <summary>
        ///     Tries to find the currently selected mapset if it is in the pool
        /// </summary>
        /// <returns></returns>
        private DrawableMapset GetCurrentlySelectedMapset()
        {
            for (var i = 0; i < Pool.Count; i++)
            {
                var item = (DrawableMapset) Pool[i];

                if (item.Item.Maps.Contains(MapManager.Selected.Value))
                    return item;
            }

            return null;
        }

        /// <summary>
        ///     Returns if the user is eligible to scroll to the middle mapset.
        ///
        ///     - The mapset must not already be selected
        ///     - The currently selected mapset must be out of view
        /// </summary>
        /// <returns></returns>
        private bool CanScrollToMiddleMapset()
        {
            var mapsetNotSelected = MiddleMapset != null && MiddleMapset.Item.Maps.First() != MapManager.Selected.Value;
            return mapsetNotSelected && GetCurrentlySelectedMapset() == null;
        }

        private void SetCaching(bool cache) => Pool.ForEach(mapset => ((DrawableMapset)mapset).DrawableContainer.IsCached = cache);
    }
}
