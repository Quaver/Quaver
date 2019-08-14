using System.Collections.Generic;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.Dropdowns
{
    public class FilterDropdownMode : LabelledDropdown
    {
        /// <summary>
        ///     The mapsets that are available to select
        /// </summary>
        private Bindable<List<Mapset>> AvailableMapsets { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="availableMapsets"></param>
        public FilterDropdownMode(Bindable<List<Mapset>> availableMapsets) : base("MODE: ", 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(120, 38), 22, ColorHelper.HexToColor($"#55ec49"), GetSelectedIndex()))
        {
            AvailableMapsets = availableMapsets;
            Dropdown.ItemSelected += OnItemSelected;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => new List<string>
        {
            "All",
            "4 Keys",
            "7 Keys",
        };

        /// <summary>
        ///     Retrieves the index of the selected value
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            return 0;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemSelected(object sender, DropdownClickedEventArgs e)
        {
        }
    }
}