using Quaver.Resources;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Screens.Select.UI.MapInfo.Banner
{
    public class MetadataContainer : Sprite
    {
        /// <summary>
        ///     The text sprite for the key of the metadata.
        /// </summary>
        private SpriteText KeyText { get; }

        /// <summary>
        ///     The text sprite for the value of the metadata.
        /// </summary>
        private SpriteText ValueText { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public MetadataContainer(string key, string value)
        {
            Tint = Color.Black;
            Alpha = 0;
            KeyText = new SpriteText(BitmapFonts.Exo2Regular, key.ToUpper() + ":", 16)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Y = 1
            };

            ValueText = new SpriteText(BitmapFonts.Exo2BoldItalic, value.ToUpper(), 16)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Y = 1
            };

            AlignText();
        }

        /// <summary>
        ///     Aligns the text property so that it looks like "Key: Value".
        /// </summary>
        private void AlignText()
        {
            KeyText.X = KeyText.Width;
            ValueText.X = ValueText.Width;
            Size = new ScalableVector2(KeyText.Width + 10 + ValueText.Width + ValueText.X / 2f, KeyText.Height);
        }

        /// <summary>
        ///     Updates the value of the metadata container.
        /// </summary>
        /// <param name="text"></param>
        public void UpdateValue(string text)
        {
            ValueText.Text = text.ToUpper();
            AlignText();
        }
    }
}