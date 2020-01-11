using Quaver.Shared.Graphics.Form.Dropdowns;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Options.Items
{
    public class OptionsItemDropdown : OptionsItem
    {
        /// <summary>
        /// </summary>
        protected Dropdown Dropdown { get; }

        public OptionsItemDropdown(float containerWidth, string name, Dropdown dropdown) : base(containerWidth, name)
        {
            Dropdown = dropdown;
            Dropdown.Parent = this;
            Dropdown.UsePreviousSpriteBatchOptions = true;
            Dropdown.Alignment = Alignment.MidRight;
            Dropdown.X = -Name.X;
        }
    }
}