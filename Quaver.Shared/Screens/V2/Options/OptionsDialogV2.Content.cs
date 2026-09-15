using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Screens.V2.Options.Catalog;
using Quaver.Shared.Screens.V2.Options.UI;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.V2.Options
{
    /// <summary>
    ///     The option rows on the right side of the menu.
    /// </summary>
    internal sealed partial class OptionsDialogV2
    {
        /// <summary>
        ///     A full-size layer that dropdown menus of option rows open into, so they draw above everything.
        /// </summary>
        private Container ContentOverlayHost { get; set; }

        private OptionsContentList ContentList { get; set; }

        private void CreateContentArea()
        {
            ContentOverlayHost = new Container
            {
                Parent = RootSurface,
                Size = RootSurface.Size
            };

            ContentList = new OptionsContentList(ContentPanel, Config.Rows, RootConfig.Shared, ContentOverlayHost);
            RebuildContent();
        }

        private void RebuildContent() => ContentList.Show(GetVisibleGroups());

        /// <summary>
        ///     The search results while searching. Otherwise the rows of the selected category,
        ///     limited to the selected subcategory.
        /// </summary>
        private IReadOnlyList<OptionsRowGroup> GetVisibleGroups()
        {
            if (SearchActive)
                return SearchResults;

            if (SelectedCategory.Id == OptionsCategoryId.RecentlyChanged)
                return OptionsRecentlyChangedTracker.GetGroups();

            var groups = OptionsContentCatalog.GetSubcategories(SelectedCategory.Id);

            if (SelectedSubcategoryKey == OptionsNavigationCatalog.AllLocalizationKey)
                return groups;

            return groups.Where(group => group.SubcategoryLocalizationKey == SelectedSubcategoryKey).ToArray();
        }
    }
}
