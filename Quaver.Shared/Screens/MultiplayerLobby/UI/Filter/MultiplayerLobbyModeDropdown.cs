using System.Collections.Generic;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Filter
{
    public class MultiplayerLobbyModeDropdown : LabelledDropdown
    {
        /// <summary>
        /// </summary>
        public MultiplayerLobbyModeDropdown() : base("MODE: ", 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(120, 38), 22, ColorHelper.HexToColor($"#55ec49"), GetSelectedIndex()))
        {
            Dropdown.ItemSelected += OnItemSelected;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            if (ConfigManager.MultiplayerLobbyGameModeType == null)
                return 0;

            return (int) ConfigManager.MultiplayerLobbyGameModeType.Value;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => new List<string>()
        {
            "All",
            "4 Keys",
            "7 Keys"
        };

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemSelected(object sender, DropdownClickedEventArgs e)
        {
            if (ConfigManager.MultiplayerLobbyGameModeType == null)
                return;

            ConfigManager.MultiplayerLobbyGameModeType.Value = (MultiplayerLobbyGameMode) e.Index;
        }
    }

    public enum MultiplayerLobbyGameMode
    {
        All,
        Keys4,
        Keys7
    }
}