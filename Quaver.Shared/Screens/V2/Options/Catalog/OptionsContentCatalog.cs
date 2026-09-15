using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Screens.V2.Options.Catalog.Categories;

namespace Quaver.Shared.Screens.V2.Options.Catalog
{
    /// <summary>
    ///     Every option row in the menu, grouped by category and subcategory.
    ///     The rows of each category are listed in the Categories folder.
    /// </summary>
    internal static class OptionsContentCatalog
    {
        private static readonly IReadOnlyDictionary<OptionsCategoryId, IReadOnlyList<OptionsRowGroup>> GroupsByCategory =
            new Dictionary<OptionsCategoryId, IReadOnlyList<OptionsRowGroup>>
            {
                [OptionsCategoryId.Video] = VideoOptions.Create(),
                [OptionsCategoryId.Audio] = AudioOptions.Create(),
                [OptionsCategoryId.Gameplay] = GameplayOptions.Create(),
                [OptionsCategoryId.Skin] = SkinOptions.Create(),
                [OptionsCategoryId.Input] = InputOptions.Create(),
                [OptionsCategoryId.Miscellaneous] = MiscellaneousOptions.Create(),
                [OptionsCategoryId.Advanced] = AdvancedOptions.Create()
            };

        /// <summary>
        ///     The subcategories of a category, in display order. Empty for "Recently Changed".
        /// </summary>
        internal static IReadOnlyList<OptionsRowGroup> GetSubcategories(OptionsCategoryId category) =>
            GroupsByCategory.TryGetValue(category, out var groups) ? groups : Array.Empty<OptionsRowGroup>();

        /// <summary>
        ///     Every row whose label contains the query, ignoring case. Rows stay grouped under the
        ///     subcategory they belong to in catalog order. Dividers are left out.
        /// </summary>
        internal static IReadOnlyList<OptionsRowGroup> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Array.Empty<OptionsRowGroup>();

            var trimmed = query.Trim();
            var results = new List<OptionsRowGroup>();

            foreach (var category in OptionsNavigationCatalog.Categories)
            {
                foreach (var group in GetSubcategories(category.Id))
                {
                    var matches = group.Rows.Where(row => Matches(row, trimmed)).ToArray();

                    if (matches.Length != 0)
                        results.Add(new OptionsRowGroup(group.Category, group.SubcategoryLocalizationKey, matches));
                }
            }

            return results;
        }

        /// <summary>
        ///     Compares against the translated label so search works the same in every language.
        /// </summary>
        private static bool Matches(OptionsRowDefinition row, string query) =>
            row.Kind != OptionsRowKind.Divider &&
            row.GetLabel().IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0;
    }
}
