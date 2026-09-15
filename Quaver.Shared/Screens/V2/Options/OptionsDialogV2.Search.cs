using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Screens.V2.Options.Catalog;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options
{
    /// <summary>
    ///     Searching. While a search is running, the content shows every matching row, the rail shows a
    ///     search entry, and the preset picker is hidden.
    /// </summary>
    internal sealed partial class OptionsDialogV2
    {
        /// <summary>
        ///     The trimmed search text. Empty when no search is running.
        /// </summary>
        private string SearchQuery { get; set; } = string.Empty;

        private IReadOnlyList<OptionsRowGroup> SearchResults { get; set; } = Array.Empty<OptionsRowGroup>();

        /// <summary>
        ///     A search with no results still counts as running until it is cleared.
        /// </summary>
        private bool SearchActive => SearchQuery.Length > 0;

        /// <summary>
        ///     Runs a search, or ends it when the text is empty. Does nothing when the text did not change,
        ///     because the search box calls this every time the user stops typing.
        /// </summary>
        /// <param name="query">The text in the search box.</param>
        /// <param name="rebuildContent">False when the caller rebuilds the content itself afterwards.</param>
        private void ApplySearch(string query, bool rebuildContent = true)
        {
            var trimmed = (query ?? string.Empty).Trim();

            if (trimmed == SearchQuery)
                return;

            var wasActive = SearchActive;
            SearchQuery = trimmed;
            SearchResults = OptionsContentCatalog.Search(trimmed);

            var resultCount = SearchResults.Sum(group => group.Rows.Count);
            SearchBar.SetResultText(LocalizationManager.Get("Screen_Options_OptionsFound", resultCount));

            ApplyPresetVisibility();
            SearchBar.Refresh(SearchActive);

            if (wasActive != SearchActive)
                CreateCategoryNavigation();
            else
                ApplyCategorySelection();

            if (rebuildContent)
                RebuildContent();
        }

        /// <param name="rebuildContent">False when the caller rebuilds the content itself afterwards.</param>
        private void ClearSearch(bool rebuildContent = true)
        {
            if (!SearchActive)
                return;

            SearchBar.ClearText();
            ApplySearch(string.Empty, rebuildContent);
        }
    }
}
