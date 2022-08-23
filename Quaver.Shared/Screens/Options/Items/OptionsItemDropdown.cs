using MonoGame.Extended;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Options.Items
{
    public class OptionsItemDropdown : OptionsItem
    {
        /// <summary>
        /// </summary>
        protected Dropdown Dropdown { get; }

        public OptionsItemDropdown(RectangleF containerRect, string name, Dropdown dropdown, string tooltipText = null) : base(containerRect, name, tooltipText)
        {
            Dropdown = dropdown;
            Dropdown.Parent = this;
            Dropdown.UsePreviousSpriteBatchOptions = true;
            Dropdown.Alignment = Alignment.MidRight;
            Dropdown.X = -Name.X;
        }
    }
}