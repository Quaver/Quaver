using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection;
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
        public FilterDropdownMode(Bindable<List<Mapset>> availableMapsets) : base(SelectionLocalization.Get("Mode:"), 20, new Dropdown(GetDropdownItems(),
            new ScalableVector2(120, 38), 18, ColorHelper.HexToColor($"#55ec49"), GetSelectedIndex()))
        {
            AvailableMapsets = availableMapsets;
            Dropdown.ItemSelected += OnItemSelected;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() {
            var values = new List<string>(){
                SelectionLocalization.Get("All")
            };

            foreach (GameMode mode in ModeHelper.AllModes)
                values.Add(ModeHelper.ToLongHand(mode));

            return values;
        }

        /// <summary>
        ///     Retrieves the index of the selected value
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            if (ConfigManager.SelectFilterGameModeBy != null)
                return (int) ConfigManager.SelectFilterGameModeBy.Value;

            return 0;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemSelected(object sender, DropdownClickedEventArgs e)
        {
            if (ConfigManager.SelectFilterGameModeBy == null)
                return;

            ConfigManager.SelectFilterGameModeBy.Value = (GameMode) e.Index;
        }
    }
}
