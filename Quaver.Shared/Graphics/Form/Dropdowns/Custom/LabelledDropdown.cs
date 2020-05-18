using System;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Form.Dropdowns.Custom
{
    public class LabelledDropdown : Container
    {
        /// <summary>
        ///     The sprite that displays the label for the dropdown
        /// </summary>
        public SpriteTextPlus Label { get; }

        /// <summary>
        /// </summary>
        public Dropdown Dropdown { get; }

        /// <summary>
        /// </summary>
        /// <param name="label"></param>
        /// <param name="fontSize"></param>
        /// <param name="dropdown"></param>
        public LabelledDropdown(string label, int fontSize, Dropdown dropdown)
        {
            Label = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), label, fontSize)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
            };

            Dropdown = dropdown;
            dropdown.Parent = this;
            dropdown.Alignment = Alignment.MidLeft;

            const int spacing = 10;

            Dropdown.X = Label.Width + spacing;
            Size = new ScalableVector2(Label.Width + spacing + Dropdown.Width, Dropdown.Height);
        }
    }
}