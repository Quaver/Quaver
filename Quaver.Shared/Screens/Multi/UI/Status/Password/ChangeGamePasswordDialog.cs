using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Multi.UI.Status.Password
{
    public class ChangeGamePasswordDialog : JoinPasswordGameDialog
    {
        public ChangeGamePasswordDialog(MultiplayerGame game) : base(game)
        {
            Textbox.OnSubmit = null;

            Header.Text = $"CHANGE GAME PASSWORD";
            Confirmation.Text = $"Enter a new password for the multiplayer game...";

            Textbox.PlaceholderText = "Enter a password...";
            Textbox.RawText = "";
            Textbox.InputText.Text = "";
            Textbox.MaxCharacters = 50;

            HandleEnterPress = false;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Enter))
            {
                OnlineManager.Client?.ChangeMultiplayerGamePassword(string.IsNullOrEmpty(Textbox.RawText) ? null : Textbox.RawText);
                Close();
            }

            base.Update(gameTime);
        }
    }
}