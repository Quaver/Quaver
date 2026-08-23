using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Server.Client.Objects.Multiplayer;

namespace Quaver.Shared.Screens.V2.Multiplayer
{
    internal sealed class MultiplayerScreen : PlaceholderScreen
    {
        public override QuaverScreenType Type { get; } = QuaverScreenType.Multiplayer;

        public MultiplayerScreen(MultiplayerGame game, bool playTrackOnFirstUpdate = false) =>
            View = new MultiplayerScreenView(this);

        public override UserClientStatus GetClientStatus() =>
            new UserClientStatus(ClientStatus.Multiplayer, -1, "", 1, "", 0);
    }
}
