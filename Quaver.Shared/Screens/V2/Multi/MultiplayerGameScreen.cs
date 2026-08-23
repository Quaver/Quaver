using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;

namespace Quaver.Shared.Screens.V2.Multi
{
    internal sealed class MultiplayerGameScreen : PlaceholderScreen, IMultiplayerGameScreenState
    {
        public override QuaverScreenType Type { get; } = QuaverScreenType.Multiplayer;

        public bool DontLeaveGameUponScreenSwitch { get; set; }

        public MultiplayerGameScreen() => View = new MultiplayerGameScreenView(this);

        public override UserClientStatus GetClientStatus() =>
            new UserClientStatus(ClientStatus.Multiplayer, -1, "-1", 1, "", 0);
    }
}
