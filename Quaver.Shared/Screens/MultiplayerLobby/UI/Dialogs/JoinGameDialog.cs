using System;
using System.Threading;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs
{
    public class JoinGameDialog : LoadingDialog
    {
        /// <summary>
        /// </summary>
        private static bool WaitingOnResponse { get; set; } = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <param name="password"></param>
        /// <param name="isCreating"></param>
        public JoinGameDialog(MultiplayerGame game, string password = null, bool isCreating = false, bool isSpectating = false) : base("JOINING GAME",
            "Connecting to multiplayer game. Please wait...", Load(game, password, isCreating, isSpectating))
        {

            OnlineManager.Client.OnJoinedMultiplayerGame += OnJoinedMultiplayerGame;
            OnlineManager.Client.OnSpectateMultiplayerGame += OnSpectateMultiplayerGame;
            OnlineManager.Client.OnJoinGameFailed += OnJoinGameFailed;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            OnlineManager.Client.OnJoinedMultiplayerGame -= OnJoinedMultiplayerGame;
            OnlineManager.Client.OnJoinGameFailed -= OnJoinGameFailed;
            OnlineManager.Client.OnSpectateMultiplayerGame -= OnSpectateMultiplayerGame;
            WaitingOnResponse = false;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <param name="password"></param>
        /// <param name="isCreating"></param>
        /// <param name="isSpectating"></param>
        /// <returns></returns>
        private static Action Load(MultiplayerGame game, string password, bool isCreating, bool isSpectating) => () =>
        {
            WaitingOnResponse = true;

            Thread.Sleep(200);

            if (!isCreating)
            {
                if (isSpectating)
                    OnlineManager.Client?.SpectateGame(game, password);
                else
                    OnlineManager.Client?.JoinGame(game, password);
            }

            while (WaitingOnResponse)
                Thread.Sleep(50);
        };

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnJoinGameFailed(object sender, JoinGameFailedEventargs e)
        {
            WaitingOnResponse = false;
            Close();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnJoinedMultiplayerGame(object sender, JoinedGameEventArgs e)
        {
            WaitingOnResponse = false;
            Close();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpectateMultiplayerGame(object sender, SpectateMultiplayerGameEventArgs e)
        {
            WaitingOnResponse = false;
            Close();
        }
    }
}