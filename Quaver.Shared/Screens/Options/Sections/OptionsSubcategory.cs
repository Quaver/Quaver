using System;
using System.Collections.Generic;
using Quaver.Shared.Screens.Options.Items;

namespace Quaver.Shared.Screens.Options.Sections
{
    public class OptionsSubcategory
    {
        /// <summary>
        ///     The name of the subcategory. Leave blank for an empty one
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// </summary>
        public List<OptionsItem> Items { get; }

        /// <summary>
        ///     Optional tooltip displayed beside the subcategory title.
        /// </summary>
        public string Tooltip { get; }

        /// <summary>
        /// </summary>
        public event EventHandler<EventArgs> ScrolledTo;

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="items"></param>
        /// <param name="tooltip"></param>
        public OptionsSubcategory(string name, List<OptionsItem> items = null, string tooltip = null)
        {
            Name = OptionsLocalization.Get(name);
            Items = items;
            Tooltip = OptionsLocalization.Get(tooltip);

            if (Items == null)
                Items = new List<OptionsItem>();
        }

        /// <summary>
        /// </summary>
        public void FireScrollToEvent() => ScrolledTo?.Invoke(this, EventArgs.Empty);
    }
}
