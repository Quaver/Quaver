using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Form
{
    public class LabelledTextarea : Sprite
    {
        /// <summary>
        /// </summary>
        public SpriteTextPlus Label { get; }

        /// <summary>
        /// </summary>
        public Textarea Textarea { get; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="label"></param>
        /// <param name="labelSize"></param>
        /// <param name="textareaHeight"></param>
        /// <param name="textareaFontSize"></param>
        /// <param name="spacing"></param>
        /// <param name="textareaPlaceholder"></param>
        /// <param name="initialText"></param>
        public LabelledTextarea(float width, string label, int labelSize = 18, int textareaHeight = 60,
            int textareaFontSize = 18, int spacing = 14, string textareaPlaceholder = "", string initialText = "")
        {
            Size = new ScalableVector2(width, 62);
            Alpha = 0;

            Label = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), label)
            {
                Parent = this,
                FontSize = labelSize
            };

            Textarea = new Textarea(new ScalableVector2(width, textareaHeight),
                FontManager.GetWobbleFont(Fonts.InterSemiBold), textareaFontSize, initialText, textareaPlaceholder)
            {
                Parent = this,
                Y = Label.Y + Label.Height + spacing,
                Tint = ColorHelper.HexToColor("#2F2F2F"),
            };

            Textarea.AddBorder(ColorHelper.HexToColor("#363636"), 2);

            Height = Label.Height + spacing + Textarea.Height;
        }
    }
}
