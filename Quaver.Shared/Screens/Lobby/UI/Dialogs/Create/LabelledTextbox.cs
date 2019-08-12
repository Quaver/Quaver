using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Screens.Lobby.UI.Dialogs.Create
{
    public class LabelledTextbox : Sprite
    {
        /// <summary>
        /// </summary>
        private SpriteTextBitmap Label { get; }

        /// <summary>
        /// </summary>
        public Textbox Textbox { get; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="label"></param>
        public LabelledTextbox(float width, string label)
        {
            Size = new ScalableVector2(width, 62);
            Alpha = 0;

            Label = new SpriteTextBitmap(FontsBitmap.GothamRegular, label)
            {
                Parent = this,
                FontSize = 17
            };

            Textbox = new Textbox(new ScalableVector2(width, 30), Fonts.SourceSansProSemiBold, 13)
            {
                Parent = this,
                Y = Label.Y + Label.Height + 14,
                Image = UserInterface.SelectSearchBackground
            };

            Textbox.AddBorder(Color.White, 2);
        }
    }
}