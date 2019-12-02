using System;
using System.Threading;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online;

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
        public JoinGameDialog(MultiplayerGame game, string password = null) : base("JOINING GAME", 
            "Connecting to multiplayer game. Please wait...", Load(game, password))
        {
            OnlineManager.Client.OnJoinedMultiplayerGame += OnJoinedMultiplayerGame;
            OnlineManager.Client.OnJoinGameFailed += OnJoinGameFailed;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            OnlineManager.Client.OnJoinedMultiplayerGame -= OnJoinedMultiplayerGame;
            OnlineManager.Client.OnJoinGameFailed -= OnJoinGameFailed;
            
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private static Action Load(MultiplayerGame game, string password) => () =>
        {
            WaitingOnResponse = true;

            Thread.Sleep(200);
            OnlineManager.Client?.JoinGame(game, password);

            while (WaitingOnResponse)
                Thread.Sleep(50);
        };
        
        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnJoinGameFailed(object sender, JoinGameFailedEventargs e) => WaitingOnResponse = false;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnJoinedMultiplayerGame(object sender, JoinedGameEventArgs e) => WaitingOnResponse = false;
    }
}