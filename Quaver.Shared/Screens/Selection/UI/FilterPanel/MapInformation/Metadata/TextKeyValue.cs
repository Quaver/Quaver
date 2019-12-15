using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata
{
    public class TextKeyValue : Container
    {
        /// <summary>
        ///     Displays the key of the metadata
        /// </summary>
        public SpriteTextPlus Key { get; private set; }

        /// <summary>
        ///     Displays the value of the metadata
        /// </summary>
        public SpriteTextPlus Value { get; private set; }

        /// <summary>
        ///     The size of the font
        /// </summary>
        private int FontSize { get; }

        /// <summary>
        ///     The color of the key
        /// </summary>
        private Color KeyColor { get; }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="fontSize"></param>
        /// <param name="keyColor"></param>
        public TextKeyValue(string key, string value, int fontSize, Color keyColor)
        {
            FontSize = fontSize;
            KeyColor = keyColor;

            CreateKey(key);
            CreateValue(value);

            Size = new ScalableVector2(Key.Width + Value.Width + 6, Key.Height);
        }

        /// <summary>
        ///     Creates <see cref="Key"/>
        /// </summary>
        private void CreateKey(string key)
        {
            Key = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), key, FontSize)
            {
                Parent = this,
            };
        }

        /// <summary>
        ///      Creases <see cref="Value"/>
        /// </summary>
        private void CreateValue(string value)
        {
            Value = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), value, FontSize)
            {
                Parent = this,
                X = Key.Width  + 6,
                Tint = KeyColor
            };
        }

        /// <summary>
        ///     Changes the value of the container
        /// </summary>
        /// <param name="value"></param>
        public void ChangeValue(string value)
        {
            Value.Text = value;
            UpdateSize();
        }

        /// <summary>
        ///     Updates the size of the container
        /// </summary>
        public virtual void UpdateSize() => Size = new ScalableVector2(Key.Width + Value.Width + 6, Key.Height);

        public void RemoveAnimations()
        {
            ClearAnimations();
            Children.ForEach(x => x.ClearAnimations());
        }

        public void FadeTo(float fade, Easing easing, int time)
        {
            Children.ForEach(x =>
            {
                if (x is Sprite s)
                    s.FadeTo(fade, easing, time);
            });
        }
    }
}