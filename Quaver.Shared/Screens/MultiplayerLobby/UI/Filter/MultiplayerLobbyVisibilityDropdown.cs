using System.Collections.Generic;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Filter
{
    public class MultiplayerLobbyVisibilityDropdown : LabelledDropdown
    {
        /// <summary>
        /// </summary>
        public MultiplayerLobbyVisibilityDropdown() : base("ROOM VISIBILITY: ", 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(140, 38), 22, ColorHelper.HexToColor($"#eb4dfa"), GetSelectedIndex()))
        {
            Dropdown.ItemSelected += OnItemSelected;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            if (ConfigManager.MultiplayerLobbyVisibilityType == null)
                return 0;

            return (int) ConfigManager.MultiplayerLobbyVisibilityType.Value;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => new List<string>
        {
            "All",
            "Open",
            "Full",
            "Password"
        };

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnItemSelected(object sender, DropdownClickedEventArgs e)
        {
            if (ConfigManager.MultiplayerLobbyVisibilityType == null)
                return;

            ConfigManager.MultiplayerLobbyVisibilityType.Value = (MultiplayerLobbyRoomVisibility) e.Index;
        }
    }

    public enum MultiplayerLobbyRoomVisibility
    {
        All,
        Open,
        Full,
        Password
    }
}