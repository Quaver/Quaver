namespace Quaver.Shared.Screens.V2.MultiplayerLobby
{
    internal sealed class MultiplayerLobbyScreenView : PlaceholderScreenView
    {
        public MultiplayerLobbyScreenView(MultiplayerLobbyScreen screen) : base(screen,
            config => config.Screens.MultiplayerLobby, "Screens.MultiplayerLobby",
            "Screen_V2_MultiplayerLobbyScreen")
        {
        }
    }
}
