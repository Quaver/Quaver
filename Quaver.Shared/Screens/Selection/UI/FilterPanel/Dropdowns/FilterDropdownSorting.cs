using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Selection;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.Dropdowns
{
    public class FilterDropdownSorting : LabelledDropdown
    {
        /// <summary>
        ///     The mapsets that are available to select
        /// </summary>
        private Bindable<List<Mapset>> AvailableMapsets { get; }

        private static List<(string Name, OrderMapsetsBy Value)> DropdownItems { get; } = new List<(string, OrderMapsetsBy)>()
        {
            ("Artist", OrderMapsetsBy.Artist),
            ("Title", OrderMapsetsBy.Title),
            ("Creator", OrderMapsetsBy.Creator),
            ("Date Added", OrderMapsetsBy.DateAdded),
            ("Ranked Status", OrderMapsetsBy.Status),
            ("BPM", OrderMapsetsBy.BPM),
            ("Times Played", OrderMapsetsBy.TimesPlayed),
            ("Recently Played", OrderMapsetsBy.RecentlyPlayed),
            ("Game", OrderMapsetsBy.Game),
            ("Length", OrderMapsetsBy.Length),
            ("Source", OrderMapsetsBy.Source),
            ("Difficulty", OrderMapsetsBy.Difficulty),
            ("Online Grade", OrderMapsetsBy.OnlineGrade),
            ("Long Note %", OrderMapsetsBy.LongNotePercentage),
            ("Notes Per Sec.", OrderMapsetsBy.NotesPerSecond)
        };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="availableMapsets"></param>
        public FilterDropdownSorting(Bindable<List<Mapset>> availableMapsets) : base(SelectionLocalization.Get("Sort By:"), 20, new Dropdown(GetDropdownItems(),
            new ScalableVector2(188, 38), 20, ColorHelper.HexToColor($"#ffe76b"), GetSelectedIndex()))
        {
            AvailableMapsets = availableMapsets;
            Dropdown.ItemSelected += OnItemSelected;
        }

        /// <summary>
        ///     Retrieves a list of dropdown items
        ///
        ///     TODO: Localize this
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => DropdownItems.Select(x => SelectionLocalization.Get(x.Name)).ToList();

        /// <summary>
        ///     Retrieves the index of the selected value
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            if (ConfigManager.SelectOrderMapsetsBy == null)
                return 0;

            var index = DropdownItems.FindIndex(x => x.Value == ConfigManager.SelectOrderMapsetsBy.Value);

            return index == -1 ? 0 : index;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemSelected(object sender, DropdownClickedEventArgs e)
        {
            if (ConfigManager.SelectOrderMapsetsBy == null)
                return;

            ConfigManager.SelectOrderMapsetsBy.Value = DropdownItems[e.Index].Value;
        }
    }
}
