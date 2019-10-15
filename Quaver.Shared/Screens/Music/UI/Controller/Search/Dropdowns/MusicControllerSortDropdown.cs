using System;
using System.Collections.Generic;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Wobble.Graphics;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Music.UI.Controller.Search.Dropdowns
{
    public class MusicControllerSortDropdown : LabelledDropdown
    {
        public MusicControllerSortDropdown() : base("SORT BY: ", 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(160, 38), 22, ColorHelper.HexToColor($"#ffe76b"), GetSelectedIndex()))
        {
            Dropdown.ItemSelected += OnItemSelected;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => new List<string>
        {
            "Artist",
            "Title",
            "Creator",
            "BPM",
            "Length"
        };

        /// <summary>
        ///     Retrieves the index of the selected value
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            return GetDropdownItems().IndexOf(ConfigManager.MusicPlayerOrderMapsBy.Value.ToString());
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemSelected(object sender, DropdownClickedEventArgs e)
        {
            var val = (OrderMapsetsBy) Enum.Parse(typeof(OrderMapsetsBy), e.Text);
            ConfigManager.MusicPlayerOrderMapsBy.Value = val;
        }
    }
}