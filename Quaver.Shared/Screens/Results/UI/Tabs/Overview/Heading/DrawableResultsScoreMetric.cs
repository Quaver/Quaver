using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Heading
{
    public class DrawableResultsScoreMetric : Sprite
    {
        /// <summary>
        /// </summary>
        private Texture2D Label { get; }

        private string Key { get; }

        /// <summary>
        /// </summary>
        private string Value { get; }

        /// <summary>
        /// </summary>
        private Color TextColor { get; }

        /// <summary>
        /// </summary>
        private Sprite LabelSprite { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus KeyText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus ValueText { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="color"></param>
        public DrawableResultsScoreMetric(string key, string value, Color? color = null)
        {
            Label = SkinManager.Skin?.Results?.ResultsLabelBackground ??
                    UserInterface.ResultsLabelBackground;
            Key = key;
            Value = value;
            TextColor = color ?? Color.White;

            Alpha = 0f;
            Size = new ScalableVector2(256, 70);
            CreateLabel();
            CreateValueText();
        }

        /// <summary>
        /// </summary>
        private void CreateLabel()
        {
            LabelSprite = new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Image = Label,
                Size = new ScalableVector2(Label.Width, Label.Height)
            };
            KeyText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), Key, 16)
            {
                Parent = LabelSprite, Alignment = Alignment.MidCenter, Tint = TextColor
            };
        }

        /// <summary>
        /// </summary>
        private void CreateValueText() => ValueText = new SpriteTextPlus(
            FontManager.GetWobbleFont(Fonts.InterBold),
            Value, 32) { Parent = this, Alignment = Alignment.TopCenter, Tint = TextColor };
    }
}