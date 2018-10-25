using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Server.Client;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.Menu.UI.Navigation
{
    public class NavbarItem : Button
    {
        /// <summary>
        ///     The bottom line that displays when the item is selected/hovered.
        /// </summary>
        public Sprite BottomLine { get; private set; }

        /// <summary>
        ///     If the item is currently selected.
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// </summary>
        public NavbarItem()
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Icon + Name
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="name"></param>
        /// <param name="selected"></param>
        /// <param name="clickAction"></param>
        public NavbarItem(string name, bool selected = false, EventHandler clickAction = null) : base(clickAction)
        {
            Selected = selected;

            UsePreviousSpriteBatchOptions = true;
            Size = new ScalableVector2(175, 45);
            Tint = Color.Black;

            Alpha = Selected ? 0.25f: 0;

            var text = new SpriteText(BitmapFonts.Exo2SemiBold, name, 13)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Y = 2
            };

            CreateBottomLine();
        }

        /// <summary>
        ///     Only icon
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="selected"></param>
        /// <param name="clickAction"></param>
        public NavbarItem(Texture2D icon, bool selected = false, EventHandler clickAction = null) : base(clickAction)
        {
            Selected = selected;
            UsePreviousSpriteBatchOptions = true;
            Tint = Color.Black;
            Alpha = Selected ? 0.25f: 0;
            Size = new ScalableVector2(45, 45);

            var centerIcon = new Sprite()
            {
                Parent = this,
                Image = icon,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(Width * 0.65f, Height * 0.65f),
            };

            CreateBottomLine();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            if (BottomLine != null)
            {
                if (Selected || IsHovered)
                    BottomLine.Width = MathHelper.Lerp(BottomLine.Width, Width, (float) Math.Min(dt / 60, 1));
                else
                    BottomLine.Width = MathHelper.Lerp(BottomLine.Width, 0, (float) Math.Min(dt / 60, 1));
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the bottom line for the item.
        /// </summary>
        protected void CreateBottomLine() => BottomLine = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.BotCenter,
            Size = new ScalableVector2(Selected ? Width : 0, 3),
            Y = 3,
        };
    }
}