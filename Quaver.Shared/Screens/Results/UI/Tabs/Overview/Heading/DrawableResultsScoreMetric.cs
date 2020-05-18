using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
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
        private SpriteTextPlus ValueText { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="label"></param>
        /// <param name="value"></param>
        /// <param name="color"></param>
        public DrawableResultsScoreMetric(Texture2D label, string value, Color? color = null)
        {
            Label = label;
            Value = value;
            TextColor = color ?? Color.White;

            Alpha = 0f;
            Size = new ScalableVector2(256, 70);
            CreateLabel();
            CreateValueText();
        }

        /// <summary>
        /// </summary>
        private void CreateLabel() => LabelSprite = new Sprite
        {
            Parent = this,
            Alignment = Alignment.BotCenter,
            Image = Label,
            Size = new ScalableVector2(Label.Width, Label.Height)
        };

        /// <summary>
        /// </summary>
        private void CreateValueText() => ValueText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBold),
            Value, 38)
        {
            Parent = this,
            Alignment = Alignment.TopCenter,
            Tint = TextColor
        };
    }
}