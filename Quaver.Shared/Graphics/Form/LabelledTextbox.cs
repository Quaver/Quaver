using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Form
{
    public class LabelledTextbox : Sprite
    {
        /// <summary>
        /// </summary>
        public SpriteTextPlus Label { get; }

        /// <summary>
        /// </summary>
        public Textbox Textbox { get; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="label"></param>
        /// <param name="labelSize"></param>
        /// <param name="textboxHeight"></param>
        /// <param name="textboxFontSize"></param>
        /// <param name="spacing"></param>
        /// <param name="textboxPlaceholder"></param>
        /// <param name="initialText"></param>
        public LabelledTextbox(float width, string label, int labelSize = 20, int textboxHeight = 60, int textboxFontSize = 20, int spacing = 14,
            string textboxPlaceholder = "", string initialText = "")
        {
            Size = new ScalableVector2(width, 62);
            Alpha = 0;

            Label = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), label)
            {
                Parent = this,
                FontSize = labelSize
            };

            Textbox = new Textbox(new ScalableVector2(width, textboxHeight), FontManager.GetWobbleFont(Fonts.LatoBlack),
                textboxFontSize, initialText, textboxPlaceholder)
            {
                Parent = this,
                Y = Label.Y + Label.Height + spacing,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
            };

            Textbox.AddBorder(ColorHelper.HexToColor("#363636"), 2);

            Height = Label.Height + spacing + Textbox.Height;
        }
    }
}