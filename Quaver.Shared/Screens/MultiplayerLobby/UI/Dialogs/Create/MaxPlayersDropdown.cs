using System.Collections.Generic;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs.Create
{
    public class MaxPlayersDropdown : LabelledDropdown
    {
        public MaxPlayersDropdown() : base("MAX PLAYERS: ", 21, new Dropdown(GetOptions(),
            new ScalableVector2(76, 35), 21))
        {
            Dropdown.SelectedIndex = Dropdown.Options.Count - 1;
            Dropdown.SelectedText.Text = Dropdown.Options[Dropdown.SelectedIndex];
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetOptions()
        {
            var options = new List<string>();

            for (var i = 0; i < 8; i++)
                options.Add(((i + 1) * 2).ToString());

            return options;
        }
    }
}