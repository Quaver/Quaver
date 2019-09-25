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
using Quaver.Shared.Screens.Selection.UI.Mapsets.Maps;
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
        ///     Event invoked when the mapset container has had its maps initialized
        /// </summary>
        public event EventHandler<MapsetContainerInitializedEventArgs> ContainerInitialized;

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

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable twice DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;
            AvailableMapsets.ValueChanged -= OnAvailableMapsetsChanged;
            SelectionScreen.RandomMapsetSelected -= OnRandomMapsetSelected;

            ContainerInitialized = null;

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

            // Move to the next mapset
            if (KeyboardManager.IsUniqueKeyPress(Keys.Right) || KeyboardManager.IsUniqueKeyPress(Keys.Down))
            {
                if (SelectedIndex.Value + 1 >= AvailableMapsets.Value.Count)
                    return;

                MapManager.Selected.Value = AvailableMapsets.Value[SelectedIndex.Value + 1].Maps.First();
                SelectedIndex.Value++;

                ScrollToSelected();
            }
            // Move to the previous mapset
            else if (KeyboardManager.IsUniqueKeyPress(Keys.Left) || KeyboardManager.IsUniqueKeyPress(Keys.Up))
            {
                if (SelectedIndex.Value - 1 < 0)
                    return;

                MapManager.Selected.Value = AvailableMapsets.Value[SelectedIndex.Value - 1].Maps.First();

                SelectedIndex.Value--;
                ScrollToSelected();
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
                PoolStartingIndex = GetPoolStartingIndex();

                // Recreate the object pool
                CreatePool();
                PositionAndContainPoolObjects();

                // Snap to it immediately
                SnapToSelected();
            }

            HasReinitialized = true;
            ContainerInitialized?.Invoke(this, new MapsetContainerInitializedEventArgs());
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
            ScrollToSelected();
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
        /// </summary>
        public void DestroyPool()
        {
            Pool.ForEach(x => x.Destroy());
            Pool.Clear();
        }
    }
}