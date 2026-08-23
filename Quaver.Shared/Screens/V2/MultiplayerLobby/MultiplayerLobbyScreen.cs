using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;

namespace Quaver.Shared.Screens.V2.MultiplayerLobby
{
    internal sealed class MultiplayerLobbyScreen : PlaceholderScreen
    {
        public override QuaverScreenType Type { get; } = QuaverScreenType.Lobby;

        public MultiplayerLobbyScreen() => View = new MultiplayerLobbyScreenView(this);

        public override UserClientStatus GetClientStatus() =>
            new UserClientStatus(ClientStatus.InLobby, -1, "", 1, "", 0);
    }
}
