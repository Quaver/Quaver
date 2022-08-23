using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using WebSocketSharp;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
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

        private ImageButton ToolTipArea { get; set; }

        /// <summary>
        ///     Text displayed when hovered over the container
        /// </summary>
        public virtual string ToolTipText => null;

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
            CreateTooltipArea();
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

        private void CreateTooltipArea()
        {
            ToolTipArea = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = Size,
                Alpha = 0f,
            };

            var game = GameBase.Game as QuaverGame;
            ToolTipArea.Hovered += (sender, args) =>
            {
                if (!ToolTipText.IsNullOrEmpty())
                    game?.CurrentScreen?.ActivateTooltip(new Tooltip(ToolTipText, ColorHelper.HexToColor("#5dc7f9")));
            };

            ToolTipArea.LeftHover += (sender, args) => game?.CurrentScreen?.DeactivateTooltip();
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