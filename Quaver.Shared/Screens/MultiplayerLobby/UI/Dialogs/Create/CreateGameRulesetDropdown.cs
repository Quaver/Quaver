using System.Collections.Generic;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs.Create
{
    public class CreateGameRulesetDropdown : LabelledDropdown
    {
        public CreateGameRulesetDropdown() : base("RULESET: ", 21,
            new Dropdown(GetOptions(), new ScalableVector2(166, 35), 21))
        {
        }

        private static List<string> GetOptions() => new List<string>()
        {
            "Free-For-All",
            // "Team",
            //"Battle Royale"
        };
    }
}