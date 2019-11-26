using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.MultiplayerLobby
{
    public sealed class MultiplayerLobbyScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Lobby;

        /// <summary>
        /// </summary>
        public MultiplayerLobbyScreen()
        {
            OnlineManager.Client?.JoinLobby();
            ScreenExiting += (sender, args) => OnlineManager.Client?.LeaveLobby();

            View = new MultiplayerLobbyScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.InLobby, -1, "", 1, "", 0);
    }
}