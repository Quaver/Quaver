using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.MultiplayerLobby;

namespace Quaver.Shared.Screens.Multi.UI.Dialogs
{
    public sealed class ConfirmExitToLobby : YesNoDialog
    {
        private MultiplayerGameScreen Screen { get; }

        public ConfirmExitToLobby(MultiplayerGameScreen screen) : base("EXIT TO LOBBY",
            "Are you sure you would like to exit to the lobby?")
        {
            Screen = screen;
            YesAction += () =>
            {
                if (Screen == null || Screen.Exiting)
                    return;

                Screen.Exit(() => new MultiplayerLobbyScreen());
            };
        }
    }
}