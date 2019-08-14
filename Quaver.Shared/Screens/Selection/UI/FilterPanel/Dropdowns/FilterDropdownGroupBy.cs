using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Lobby.UI.Dialogs.Create;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.Dropdowns
{
    public class FilterDropdownGroupBy : LabelledDropdown
    {
        /// <summary>
        ///     The mapsets that are available to select
        /// </summary>
        private Bindable<List<Mapset>> AvailableMapsets { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="availableMapsets"></param>
        public FilterDropdownGroupBy(Bindable<List<Mapset>> availableMapsets) : base("GROUP BY: ", 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(125, 38), 22, ColorHelper.HexToColor($"#10C8F6"), GetSelectedIndex()))
        {
            AvailableMapsets = availableMapsets;
            Dropdown.ItemSelected += OnItemSelected;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => new List<string>
        {
            "None",
            "Playlists",
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