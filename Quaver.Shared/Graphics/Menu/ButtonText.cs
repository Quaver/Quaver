using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Shared.Graphics.Menu
{
    public class ButtonText : Button
    {
        /// <summary>
        /// </summary>
        public SpriteTextBitmap Text { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="onClicked"></param>
        public ButtonText(BitmapFont font, string text, int fontSize, EventHandler onClicked = null)
        {
            Text = new SpriteTextBitmap(font, text.ToUpper())
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                FontSize = fontSize
            };

            Size = Text.Size;
            Tint = Color.Transparent;

            if (onClicked != null)
                Clicked += onClicked;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            FadeTextToColor(IsHovered ? Colors.MainAccent : Color.White, gameTime.ElapsedGameTime.TotalMilliseconds, 60);
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="color"></param>
        /// <param name="dt"></param>
        /// <param name="scale"></param>
        private void FadeTextToColor(Color color, double dt, int scale)
        {
            var r = MathHelper.Lerp(Text.Tint.R, color.R, (float) Math.Min(dt / scale, 1));
            var g = MathHelper.Lerp(Text.Tint.G, color.G, (float) Math.Min(dt / scale, 1));
            var b = MathHelper.Lerp(Text.Tint.B, color.B, (float) Math.Min(dt / scale, 1));

            Text.Tint = new Color((int)r, (int)g, (int)b);
        }

        public void ChangeText(string text)
        {
            Text.Text = text;
            Size = Text.Size;
        }
    }
}