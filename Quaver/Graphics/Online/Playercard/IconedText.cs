using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Graphics.Online.Playercard
{
    public class IconedText : Sprite
    {
        /// <summary>
        ///     The sprite for the icon.
        /// </summary>
        public Sprite Icon { get; }

        /// <summary>
        ///     The text for the icon.
        /// </summary>
        public SpriteTextBitmap Value { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        public IconedText(Texture2D icon, string text)
        {
            Alpha = 0;

            Icon = new Sprite
            {
                Parent = this,
                Image = icon,
                Size = new ScalableVector2(20, 20),
                UsePreviousSpriteBatchOptions = true,
            };

            Value = new SpriteTextBitmap(BitmapFonts.Exo2BoldItalic, " ", 24, Color.White, Alignment.MidCenter, int.MaxValue)
            {
                Parent = Icon,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true,
                X = Icon.Width + 2,
                Y = 2
            };

            UpdateValue(text);
        }

        /// <summary>
        ///     Updates the value of the text.
        /// </summary>
        /// <param name="text"></param>
        public void UpdateValue(string text)
        {
            Value.Text = text;
            Value.Size = new ScalableVector2(Value.Width * 0.45f, Value.Height * 0.45f);

            // Calculate the size of the entire thing
            Size = new ScalableVector2(Value.X + Value.Width, Icon.Height);
        }
    }
}