using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs;
using Wobble.Graphics.Animations;

namespace Quaver.Shared.Screens.Multi.UI.Status.Name
{
    public class ChangeGameNameDialog : JoinPasswordGameDialog
    {
        public ChangeGameNameDialog(MultiplayerGame game) : base(game)
        {
            Textbox.OnSubmit = null;

            Header.Text = $"CHANGE GAME NAME";
            Confirmation.Text = $"Enter a new name for the multiplayer game...";

            Textbox.PlaceholderText = "Enter a name...";
            Textbox.RawText = game.Name;
            Textbox.InputText.Text = game.Name;
            Textbox.MaxCharacters = 50;

            HandleEnterPress = false;

            Textbox.OnSubmit += s =>
            {
                if (Textbox.RawText != game.Name)
                    OnlineManager.Client?.ChangeMultiplayerGameName(Textbox.RawText);

                Close();
            };
        }
    }
}