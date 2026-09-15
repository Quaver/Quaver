using Quaver.Shared.Screens.V2.Options.UI;

namespace Quaver.Shared.Screens.V2.Options.Catalog
{
    internal enum OptionsCategoryId
    {
        /// <summary>
        ///     Shows the rows the user changed most recently. It has no rows of its own.
        /// </summary>
        RecentlyChanged,
        Video,
        Audio,
        Gameplay,
        Skin,
        Input,
        Miscellaneous,
        Advanced
    }

    /// <summary>
    ///     A category button in the left rail.
    /// </summary>
    internal sealed class OptionsCategoryDefinition
    {
        internal OptionsCategoryId Id { get; }

        internal string LocalizationKey { get; }

        /// <summary>
        ///     The category's icon in the icon atlas
        /// </summary>
        internal OptionsIconFrame? Icon { get; }

        internal OptionsCategoryDefinition(OptionsCategoryId id, string localizationKey, OptionsIconFrame? icon)
        {
            Id = id;
            LocalizationKey = localizationKey;
            Icon = icon;
        }
    }
}
