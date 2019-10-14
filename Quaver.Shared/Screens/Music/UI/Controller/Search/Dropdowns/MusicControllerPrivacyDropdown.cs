using System.Collections.Generic;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Wobble.Graphics;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Music.UI.Controller.Search.Dropdowns
{
    public class MusicControllerPrivacyDropdown : LabelledDropdown
    {
        public MusicControllerPrivacyDropdown() : base("PRIVACY: ", 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(160, 38), 22, ColorHelper.HexToColor($"#10C8F6"), GetSelectedIndex()))
        {
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => new List<string>
        {
            "Open",
            "Friends",
            "Invite-Only",
            "Closed"
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