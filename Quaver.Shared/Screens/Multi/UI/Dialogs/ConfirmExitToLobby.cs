using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.MultiplayerLobby;

namespace Quaver.Shared.Screens.Multi.UI.Dialogs
{
    public sealed class ConfirmExitToLobby : YesNoDialog
    {
        private MultiplayerGameScreen Screen { get; }

        public ConfirmExitToLobby(MultiplayerGameScreen screen) : base(MultiLocalization.Get("ExitToLobbyTitle"),
            MultiLocalization.Get("ExitToLobbyMessage"))
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
