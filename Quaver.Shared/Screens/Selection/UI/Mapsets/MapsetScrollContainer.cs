using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
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
    public class MapsetScrollContainer : PoolableScrollContainer<Mapset>
    {
        /// <summary>
        /// </summary>
        private Bindable<List<Mapset>> AvailableMapsets { get; }

        /// <summary>
        /// </summary>
        private Sprite ScrollbarBackground { get; set; }

        /// <summary>
        ///     The index of the currently selected map
        /// </summary>
        public int SelectedMapsetIndex { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="availableMapsets"></param>
        public MapsetScrollContainer(Bindable<List<Mapset>> availableMapsets) : base(availableMapsets.Value, 12, 0,
            new ScalableVector2(DrawableMapset.WIDTH, 880), new ScalableVector2(DrawableMapset.WIDTH, 1000))
        {
            AvailableMapsets = availableMapsets;
            PaddingBottom = 10;

            InputEnabled = true;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 320;

            Alpha = 0;
            CreateScrollbar();

            CreatePool();

            MapManager.Selected.ValueChanged += OnMapChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && DialogManager.Dialogs.Count == 0
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

            HandleInput();

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateScrollbar()
        {
            ScrollbarBackground = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = 30,
                Size = new ScalableVector2(4, Height - 50),
                Tint = ColorHelper.HexToColor("#474747")
            };

            MinScrollBarY = -805 - (int) Scrollbar.Height / 2;
            Scrollbar.Width = ScrollbarBackground.Width;
            Scrollbar.Parent = ScrollbarBackground;
            Scrollbar.Alignment = Alignment.BotCenter;
            Scrollbar.Tint = Color.White;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<Mapset> CreateObject(Mapset item, int index) => new DrawableMapset(this, item, index);

        /// <summary>
        /// </summary>
        private void HandleInput()
        {
            // Advance to next map
            if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
            {
                if (SelectedMapsetIndex + 1 < AvailableMapsets.Value.Count)
                    MapManager.Selected.Value = AvailableMapsets.Value[SelectedMapsetIndex + 1].Maps.First();

                SelectedMapsetIndex++;

                ScrollToMapset();
            }
            // Go back to previous map
            else if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
            {
                if (SelectedMapsetIndex - 1 >= 0)
                    MapManager.Selected.Value = AvailableMapsets.Value[SelectedMapsetIndex - 1].Maps.First();

                SelectedMapsetIndex--;

                ScrollToMapset();
            }
        }

        /// <summary>
        /// </summary>
        private void ScrollToMapset()
        {
            if (SelectedMapsetIndex < 3)
                return;

            // Scroll the the place where the map is.
            var targetScroll = (-SelectedMapsetIndex + 4) * DrawableMapset.MapsetHeight + (-SelectedMapsetIndex - 3);
            ScrollTo(targetScroll, 1800);
        }

        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => ScrollToMapset();
    }
}