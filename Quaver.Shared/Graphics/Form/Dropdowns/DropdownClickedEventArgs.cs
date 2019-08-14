using System;
using System.Collections.Generic;

namespace Quaver.Shared.Graphics.Form.Dropdowns
{
    public class DropdownClickedEventArgs : EventArgs
    {
        /// <summary>
        ///     The item that was clicked
        /// </summary>
        public DropdownItem Item { get; }

        /// <summary>
        ///     The list of options in the dropdown
        /// </summary>
        public List<string> Options => Item.Dropdown.Options;
        
        /// <summary>
        ///     The index of <see cref="Options"/> that was selected
        /// </summary>
        public int Index => Item.Index;

        /// <summary>
        ///     The text of the item that was selected
        /// </summary>
        public string Text => Item.Text.Text;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        public DropdownClickedEventArgs(DropdownItem item) => Item = item;
    }
}