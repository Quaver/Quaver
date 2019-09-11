using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Selection.UI.Mapsets.Maps;
using TagLib.Ape;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

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
        protected override float GetSelectedPosition() => (-SelectedIndex + 4) * DrawableMapset.MapsetHeight;

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
            if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
            {
                if (SelectedIndex + 1 >= AvailableMapsets.Value.Count)
                    return;

                MapManager.Selected.Value = AvailableMapsets.Value[SelectedIndex + 1].Maps.First();
                SelectedIndex++;

                ScrollToSelected();
            }
            // Move to the previous mapset
            else if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
            {
                if (SelectedIndex - 1 < 0)
                    return;

                MapManager.Selected.Value = AvailableMapsets.Value[SelectedIndex - 1].Maps.First();

                SelectedIndex--;
                ScrollToSelected();
            }
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
        private void OnAvailableMapsetsChanged(object sender, BindableValueChangedEventArgs<List<Mapset>> e)
        {
            ThreadScheduler.RunAfter(() =>
            {
                lock (Pool)
                {
                    DestroyAndClearPool();

                    AvailableItems = e.Value;

                    SelectedIndex = e.Value.FindIndex(x => x.Maps.Contains(MapManager.Selected.Value));

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
            }, 250);
        }
    }
}