using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Online;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemInProgress : MultiplayerTableItem
    {
        public MultiplayerTableItemInProgress(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnGameStarted += OnGameStarted;
                OnlineManager.Client.OnGameEnded += OnGameEnded;
            }
        }
        
        public override string GetName() => "In Progress";

        public override string GetValue() => SelectedGame.Value.InProgress ? "Yes" : "No";
        
        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameStarted(object sender, GameStartedEventArgs e) => NeedsStateUpdate = true;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameEnded(object sender, GameEndedEventArgs e) => NeedsStateUpdate = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Dispose()
        {
            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnGameStarted -= OnGameStarted;
                OnlineManager.Client.OnGameEnded -= OnGameEnded;
            }
            
            base.Dispose();
        }
    }
}