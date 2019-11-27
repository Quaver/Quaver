using System.Collections.Generic;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Online;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby
{
    public sealed class MultiplayerLobbyScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Lobby;

        /// <summary>
        ///    The currently visible multiplayer games
        /// </summary>
        public Bindable<List<MultiplayerGame>> VisibleGames { get; private set; }

        /// <summary>
        ///     The currently selected multiplayer game
        /// </summary>
        public Bindable<MultiplayerGame> SelectedGame { get; private set; }

        /// <summary>
        /// </summary>
        public MultiplayerLobbyScreen()
        {
            CreateBindableVisibleGames();
            CreateBindableSelectedGame();

            OnlineManager.Client?.JoinLobby();
            ScreenExiting += (sender, args) => OnlineManager.Client?.LeaveLobby();

            View = new MultiplayerLobbyScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            VisibleGames.Dispose();
            SelectedGame.Dispose();

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBindableVisibleGames() => VisibleGames = new Bindable<List<MultiplayerGame>>(new List<MultiplayerGame>())
        {
            Value = new List<MultiplayerGame>()
        };

        /// <summary>
        /// </summary>
        private void CreateBindableSelectedGame() => SelectedGame = new Bindable<MultiplayerGame>(null);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.InLobby, -1, "", 1, "", 0);
    }
}