using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Select;
using Wobble.Bindables;
using Wobble.Input;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps
{
    public class MapScrollContainer : SongSelectContainer<Map>
    {
        public override SelectScrollContainerType Type { get; } = SelectScrollContainerType.Maps;

        /// <summary>
        ///     The currently active container in song select
        /// </summary>
        public Bindable<SelectScrollContainerType> ActiveScrollContainer { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="availableItems"></param>
        /// <param name="activeContainer"></param>
        public MapScrollContainer(List<Map> availableItems, Bindable<SelectScrollContainerType> activeContainer) : base(availableItems, 12)
        {
            MapManager.Selected.ValueChanged += OnMapChanged;
            ActiveScrollContainer = activeContainer;
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
        protected override float GetSelectedPosition() => (-SelectedIndex + 4) * DrawableMapset.MapsetHeight + (-SelectedIndex - 3);

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
            if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
            {
                if (SelectedIndex + 1 >= MapManager.Selected.Value.Mapset.Maps.Count)
                    return;

                MapManager.Selected.Value = MapManager.Selected.Value.Mapset.Maps[SelectedIndex + 1];
                SelectedIndex++;

                ScrollToSelected();
            }
            // Move to the previous mapset
            else if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
            {
                if (SelectedIndex - 1 < 0)
                    return;

                MapManager.Selected.Value = MapManager.Selected.Value.Mapset.Maps[SelectedIndex - 1];

                SelectedIndex--;
                ScrollToSelected();
            }
        }

        /// <summary>
        ///     Called when the selected map has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            if (e.Value.Mapset == e.OldValue.Mapset)
                return;

            DestroyAndClearPool();

            AvailableItems = e.Value.Mapset.Maps;

            // Get the new selected mapset index
            SelectedIndex = e.Value.Mapset.Maps.FindIndex(x => x == MapManager.Selected.Value);

            if (SelectedIndex == -1)
                SelectedIndex = 0;

            // Reset the starting index so we can be aware of the mapsets that are needed
            PoolStartingIndex = GetPoolStartingIndex();

            // Recreate the object pool
            CreatePool();
            PositionAndContainPoolObjects();

            // Snap to it immediately
            SnapToSelected();
        }
    }
}