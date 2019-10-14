using System.Collections.Generic;
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
            return 0;
        }
    }
}