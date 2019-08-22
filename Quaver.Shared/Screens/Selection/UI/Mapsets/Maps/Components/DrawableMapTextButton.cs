using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Components
{
    public class DrawableMapTextButton : ImageButton, IDrawableMapComponent
    {
        /// <summary>
        /// </summary>
        private SpriteTextPlus Text { get; }

        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public DrawableMapTextButton(Map map, string text, Color color) : base(UserInterface.EditPlayButton)
        {
            Map = map;
            Tint = color;
            Size = new ScalableVector2(195, 30);

            Text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), text, 20)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Alpha = 0,
                Visible = false
            };

            SetChildrenAlpha = true;
            SetChildrenVisibility = true;

            MapManager.Selected.ValueChanged += OnMapChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            MapManager.Selected.ValueChanged += OnMapChanged;
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Open() => Open(false);

        /// <summary>
        /// </summary>
        public void Open(bool instant)
        {
            IsClickable = true;

            ClearAnimations();

            if (MapManager.Selected.Value != Map)
                return;

            Visible = true;

            if (instant)
            {
                Alpha = 1;
                return;
            }

            Alpha = 0;
            Wait(200);
            FadeTo(1, Easing.Linear, 250);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Close()
        {
            IsClickable = false;

            ClearAnimations();
            Alpha = 0;
            Visible = false;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            if (MapManager.Selected.Value == Map)
                Open(true);
            else
                Close();
        }
    }
}