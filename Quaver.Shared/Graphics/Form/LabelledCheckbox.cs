using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Form
{
    public class LabelledCheckbox : Container
    {
        /// <summary>
        ///     The sprite that displays the label for the dropdown
        /// </summary>
        public SpriteTextPlus Label { get; }

        /// <summary>
        /// </summary>
        public Checkbox Checkbox { get; }

        /// <summary>
        /// </summary>
        /// <param name="label"></param>
        /// <param name="fontSize"></param>
        /// <param name="checkbox"></param>
        public LabelledCheckbox(string label, int fontSize, Checkbox checkbox)
        {
            Label = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), label, fontSize)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
            };

            Checkbox = checkbox;
            Checkbox.Parent = this;
            Checkbox.Alignment = Alignment.MidLeft;

            const int spacing = 14;

            Checkbox.X = Label.Width + spacing;
            Size = new ScalableVector2(Label.Width + spacing + Checkbox.Width, Math.Max(Checkbox.Height, Label.Height));
        }
    }
}