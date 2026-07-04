using System.Collections.Generic;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Filter
{
    public class MultiplayerLobbyMapDropdown : LabelledDropdown
    {
        public MultiplayerLobbyMapDropdown() : base(MultiplayerLobbyLocalization.Get("MapStatusLabel"), 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(168, 38), 22, ColorHelper.HexToColor($"#10C8F6"), GetSelectedIndex()))
        {
            Dropdown.ItemSelected += OnItemSelected;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            if (ConfigManager.MultiplayerLobbyMapStatusType == null)
                return 0;

            return (int) ConfigManager.MultiplayerLobbyMapStatusType.Value;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => new List<string>()
        {
            MultiplayerLobbyLocalization.Get("All"),
            MultiplayerLobbyLocalization.Get("Imported"),
            MultiplayerLobbyLocalization.Get("Uploaded"),
            MultiplayerLobbyLocalization.Get("Unsubmitted")
        };

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemSelected(object sender, DropdownClickedEventArgs e)
        {
            if (ConfigManager.MultiplayerLobbyMapStatusType == null)
                return;

            ConfigManager.MultiplayerLobbyMapStatusType.Value = (MultiplayerLobbyMapStatus) e.Index;
        }
    }

    public enum MultiplayerLobbyMapStatus
    {
        All,
        Imported,
        Uploaded,
        Unsubmitted
    }
}
