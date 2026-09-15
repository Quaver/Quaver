using System.Collections.Generic;

namespace Quaver.Shared.Screens.V2.Options.Catalog
{
    /// <summary>
    ///     A list of rows shown under one section header, for example "Audio Settings | Volume".
    ///     The catalog stores every subcategory this way. Search results and recently changed rows
    ///     use it too, so each row keeps the header of the place it normally lives.
    /// </summary>
    internal sealed class OptionsRowGroup
    {
        internal OptionsCategoryId Category { get; }

        internal string SubcategoryLocalizationKey { get; }

        internal IReadOnlyList<OptionsRowDefinition> Rows { get; }

        internal OptionsRowGroup(OptionsCategoryId category, string subcategoryLocalizationKey,
            params OptionsRowDefinition[] rows)
        {
            Category = category;
            SubcategoryLocalizationKey = subcategoryLocalizationKey;
            Rows = rows;
        }
    }
}
