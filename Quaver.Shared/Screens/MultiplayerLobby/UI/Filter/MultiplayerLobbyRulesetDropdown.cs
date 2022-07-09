using System.Collections.Generic;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Filter
{
    public class MultiplayerLobbyRulesetDropdown : LabelledDropdown
    {
        /// <summary>
        /// </summary>
        public MultiplayerLobbyRulesetDropdown() : base("RULESET: ", 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(170, 38), 22, ColorHelper.HexToColor($"#ffe76b"), GetSelectedIndex()))
        {
            Dropdown.ItemSelected += OnItemSelected;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            if (ConfigManager.MultiplayerLobbyRulesetType == null)
                return 0;

            return (int) ConfigManager.MultiplayerLobbyRulesetType.Value;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => new List<string>
        {
            "All",
            "Free-For-All",
            "Team",
            // "Battle Royale"
        };

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnItemSelected(object sender, DropdownClickedEventArgs e)
        {
            if (ConfigManager.MultiplayerLobbyRulesetType == null)
                return;

            ConfigManager.MultiplayerLobbyRulesetType.Value = (MultiplayerLobbyRuleset) e.Index;
        }
    }

    public enum MultiplayerLobbyRuleset
    {
        All,
        FreeForAll,
        Team,
        BattleRoyale
    }
}