using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Wobble.Bindables;
using Wobble.Input;

namespace Quaver.Shared.Screens.Selection.UI.Maps
{
    public class MapScrollContainer : SongSelectContainer<Map>
    {
        public override SelectScrollContainerType Type { get; } = SelectScrollContainerType.Maps;

        /// <summary>
        ///     The currently active container in song select
        /// </summary>
        public Bindable<SelectScrollContainerType> ActiveScrollContainer { get; }

        /// <summary>
        /// </summary>
        private Bindable<List<Mapset>> AvailableMapsets { get; }

        /// <summary>
        /// </summary>
        private MapsetScrollContainer MapsetContainer { get; }

        /// <summary>
        ///     Quick reference to the current mapset.
        /// </summary>
        private Mapset CurrentMapset => AvailableMapsets?.Value?.ElementAtOrDefault(MapsetContainer.SelectedIndex.Value);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="availableMapsets"></param>
        /// <param name="mapsetContainer"></param>
        /// <param name="availableItems"></param>
        /// <param name="activeContainer"></param>
        public MapScrollContainer(Bindable<List<Mapset>> availableMapsets, MapsetScrollContainer mapsetContainer, List<Map> availableItems, Bindable<SelectScrollContainerType> activeContainer) : base(availableItems, 12)
        {
            AvailableMapsets = availableMapsets;
            MapsetContainer = mapsetContainer;

            MapsetContainer.SelectedIndex.ValueChanged += OnSelectedMapsetChanged;
            MapManager.Selected.ValueChanged += OnMapChanged;
            ActiveScrollContainer = activeContainer;

            if (CurrentMapset != null)
                Initialize(CurrentMapset.Maps);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override float GetSelectedPosition()
            => (-SelectedIndex.Value + 4) * DrawableMapset.MapsetHeight + (-SelectedIndex.Value - 3);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<Map> CreateObject(Map item, int index) => new DrawableMap(this, item, index);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void HandleInput(GameTime gameTime)
        {
            if (ActiveScrollContainer.Value != SelectScrollContainerType.Maps)
                return;

            // Move to the next mapset
            if (KeyboardManager.IsUniqueKeyPress(Keys.Right) || KeyboardManager.IsUniqueKeyPress(Keys.Down))
            {
                if (SelectedIndex.Value + 1 >= CurrentMapset.Maps.Count)
                    return;

                MapManager.Selected.Value = CurrentMapset.Maps[SelectedIndex.Value + 1];
                SelectedIndex.Value++;

                ScrollToSelected();
            }
            // Move to the previous mapset
            else if (KeyboardManager.IsUniqueKeyPress(Keys.Left) || KeyboardManager.IsUniqueKeyPress(Keys.Up))
            {
                if (SelectedIndex.Value - 1 < 0)
                    return;

                MapManager.Selected.Value = CurrentMapset.Maps[SelectedIndex.Value - 1];

                SelectedIndex.Value--;
                ScrollToSelected();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void SetSelectedIndex()
        {
            if (CurrentMapset == null)
            {
                SelectedIndex.Value = 0;
                return;
            }

            var index = CurrentMapset.Maps.FindIndex(x => x == MapManager.Selected.Value);

            if (index == -1)
                index = 0;

            SelectedIndex.Value = index;
        }

        /// <summary>
        ///     Initializes the container with new maps
        /// </summary>
        /// <param name="maps"></param>
        public void Initialize(List<Map> maps)
        {
            DestroyAndClearPool();

            AvailableItems = maps;

            // Get the new selected mapset index
            SetSelectedIndex();

            // Reset the starting index so we can be aware of the mapsets that are needed
            PoolStartingIndex = GetPoolStartingIndex();

            // Recreate the object pool
            CreatePool();
            PositionAndContainPoolObjects();

            // Snap to it immediately
            SnapToSelected();
        }

        /// <summary>
        ///     Called when the selected map has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            if (e.Value?.Mapset != e.OldValue?.Mapset)
                return;

            ScrollToSelected();
        }

        /// <summary>
        ///     Reinitializes the mapsets when changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedMapsetChanged(object sender, BindableValueChangedEventArgs<int> e)
        {
            if (CurrentMapset == null)
                return;

            Initialize(CurrentMapset.Maps);
        }
    }
}