using System.Collections.Generic;

namespace Quaver.Shared.Screens.V2.Options.Catalog
{
    /// <summary>
    ///     Remembers which rows the user changed (newest first), for the "Recently Changed" category.
    ///     The list is only kept in memory, so it starts empty every time the game starts.
    /// </summary>
    internal static class OptionsRecentlyChangedTracker
    {
        private const int MaximumEntries = 10;

        /// <summary>
        ///     Newest first.
        /// </summary>
        private static readonly List<OptionsRowDefinition> Recent = new List<OptionsRowDefinition>();

        /// <summary>
        ///     The catalog group each row belongs to. Built on first use.
        /// </summary>
        private static Dictionary<OptionsRowDefinition, OptionsRowGroup> GroupByRow;

        internal static bool HasEntries() => Recent.Count != 0;

        /// <summary>
        ///     Moves the row to the front of the list. Does nothing if it is already at the front,
        ///     so a slider that reports a change every frame while dragged is cheap.
        /// </summary>
        internal static void Record(OptionsRowDefinition row)
        {
            if (row == null || (Recent.Count > 0 && Recent[0] == row))
                return;

            EnsureIndex();

            if (!GroupByRow.ContainsKey(row))
                return;

            Recent.Remove(row);
            Recent.Insert(0, row);

            if (Recent.Count > MaximumEntries)
                Recent.RemoveRange(MaximumEntries, Recent.Count - MaximumEntries);
        }

        /// <summary>
        ///     The recorded rows (newest first). Rows next to each other in the list that come from the
        ///     same subcategory share one header.
        /// </summary>
        internal static IReadOnlyList<OptionsRowGroup> GetGroups()
        {
            EnsureIndex();

            var groups = new List<OptionsRowGroup>();
            var rows = new List<OptionsRowDefinition>();
            OptionsRowGroup source = null;

            void AddPendingGroup()
            {
                if (rows.Count == 0)
                    return;

                groups.Add(new OptionsRowGroup(source.Category, source.SubcategoryLocalizationKey, rows.ToArray()));
                rows.Clear();
            }

            foreach (var row in Recent)
            {
                var rowSource = GroupByRow[row];

                if (rowSource != source)
                    AddPendingGroup();

                source = rowSource;
                rows.Add(row);
            }

            AddPendingGroup();
            return groups;
        }

        private static void EnsureIndex()
        {
            if (GroupByRow != null)
                return;

            GroupByRow = new Dictionary<OptionsRowDefinition, OptionsRowGroup>();

            foreach (var category in OptionsNavigationCatalog.Categories)
            {
                foreach (var group in OptionsContentCatalog.GetSubcategories(category.Id))
                {
                    foreach (var row in group.Rows)
                    {
                        if (row.Kind != OptionsRowKind.Divider)
                            GroupByRow[row] = group;
                    }
                }
            }
        }
    }
}
