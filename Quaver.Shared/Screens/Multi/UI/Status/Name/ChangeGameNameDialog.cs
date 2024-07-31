using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs;

namespace Quaver.Shared.Screens.Multi.UI.Status.Name
{
    public class ChangeGameNameDialog : JoinPasswordGameDialog
    {
        public ChangeGameNameDialog(MultiplayerGame game) : base(game, initialText: game.Name)
        {
            Textbox.OnSubmit = null;

            Header.Text = $"CHANGE GAME NAME";
            Confirmation.Text = $"Enter a new name for the multiplayer game...";

            Textbox.PlaceholderText = "Enter a name...";
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