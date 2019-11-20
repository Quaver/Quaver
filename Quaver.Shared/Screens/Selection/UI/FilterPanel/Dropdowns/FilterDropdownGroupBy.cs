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
            ConfigManager.SelectGroupMapsetsBy.ValueChanged += OnGroupingChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            ConfigManager.SelectGroupMapsetsBy.ValueChanged -= OnGroupingChanged;
            base.Destroy();
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
            if (ConfigManager.SelectGroupMapsetsBy == null)
                return 0;

            return (int) ConfigManager.SelectGroupMapsetsBy.Value;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemSelected(object sender, DropdownClickedEventArgs e)
        {
            if (ConfigManager.SelectGroupMapsetsBy == null)
                return;

            ConfigManager.SelectGroupMapsetsBy.Value = (GroupMapsetsBy) e.Index;
        }


        /// <summary>
        ///     Called when <see cref="ConfigManager.SelectGroupMapsetsBy"/> has changed, so the state
        ///     of the dropdown can be updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnGroupingChanged(object sender, BindableValueChangedEventArgs<GroupMapsetsBy> e)
        {
            Dropdown.SelectedIndex = (int) e.Value;
            Dropdown.SelectedText.Text = Dropdown.Options[(int) e.Value];
            Dropdown.Close();
        }
    }
}