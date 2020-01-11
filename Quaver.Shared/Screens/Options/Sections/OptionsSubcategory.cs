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
        /// </summary>
        /// <param name="name"></param>
        /// <param name="items"></param>
        public OptionsSubcategory(string name, List<OptionsItem> items = null)
        {
            Name = name;
            Items = items;

            if (Items == null)
                Items = new List<OptionsItem>();
        }
    }
}