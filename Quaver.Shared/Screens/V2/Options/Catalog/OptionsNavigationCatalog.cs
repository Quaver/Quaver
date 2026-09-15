using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Screens.V2.Options.UI;

namespace Quaver.Shared.Screens.V2.Options.Catalog
{
    /// <summary>
    ///     The categories shown in the left rail, in order.
    /// </summary>
    internal static class OptionsNavigationCatalog
    {
        /// <summary>
        ///     The "All" entry at the top of every subcategory list.
        /// </summary>
        internal const string AllLocalizationKey = "Screen_Selection_All";

        /// <summary>
        ///     Pinned above the other categories. A search result entry takes its place while searching.
        /// </summary>
        internal static OptionsCategoryDefinition RecentlyChanged { get; } =
            new OptionsCategoryDefinition(OptionsCategoryId.RecentlyChanged, "Screen_Options_RecentlyChanged",
                OptionsIconFrame.RecentlyChanged);

        internal static IReadOnlyList<OptionsCategoryDefinition> Categories { get; } = Array.AsReadOnly(new[]
        {
            new OptionsCategoryDefinition(OptionsCategoryId.Video, "Screen_Options_Video", OptionsIconFrame.Video),
            new OptionsCategoryDefinition(OptionsCategoryId.Audio, "Screen_Options_Audio", OptionsIconFrame.Audio),
            new OptionsCategoryDefinition(OptionsCategoryId.Gameplay, "Screen_Options_Gameplay",
                OptionsIconFrame.Gameplay),
            new OptionsCategoryDefinition(OptionsCategoryId.Skin, "Screen_Options_Skin", OptionsIconFrame.Skin),
            new OptionsCategoryDefinition(OptionsCategoryId.Input, "Screen_Options_Input", OptionsIconFrame.Input),
            new OptionsCategoryDefinition(OptionsCategoryId.Miscellaneous, "Screen_Options_Miscellaneous",
                OptionsIconFrame.Miscellaneous),
            new OptionsCategoryDefinition(OptionsCategoryId.Advanced, "Screen_Options_Advanced",
                OptionsIconFrame.Advanced)
        });

        internal static OptionsCategoryDefinition Get(OptionsCategoryId id) =>
            id == OptionsCategoryId.RecentlyChanged ? RecentlyChanged : Categories.First(category => category.Id == id);
    }
}
