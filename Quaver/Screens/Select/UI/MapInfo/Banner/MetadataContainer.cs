using Quaver.Assets;
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
            KeyText = new SpriteText(Fonts.Exo2Regular24, key.ToUpper() + ":")
            {
                Parent = this,
                TextScale = 0.45f,
                Alignment = Alignment.MidLeft,
                Y = 1
            };

            ValueText = new SpriteText(Fonts.Exo2BoldItalic24, value.ToUpper())
            {
                Parent = this,
                TextScale = 0.45f,
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
            KeyText.X = KeyText.MeasureString().X / 2f;
            ValueText.X = KeyText.MeasureString().X + 3 + ValueText.MeasureString().X / 2f;
            Size = new ScalableVector2(KeyText.MeasureString().X / 2f + 10 + ValueText.MeasureString().X / 2f + ValueText.X / 2f,
                KeyText.MeasureString().Y);
        }

        /// <summary>
        ///     Updates the value of the metadata container.
        /// </summary>
        /// <param name="text"></param>
        public void UpdateValue(string text, Color? textcolor = null)
        {
            ValueText.Text = text.ToUpper();
            AlignText();

            if (textcolor != null)
            {
                ValueText.TextColor = (Color)textcolor;
            }
        }
    }
}