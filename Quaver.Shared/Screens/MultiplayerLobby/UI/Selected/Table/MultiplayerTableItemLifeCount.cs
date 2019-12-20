using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Online;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemLifeCount : MultiplayerTableItem
    {
        public MultiplayerTableItemLifeCount(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
            if (OnlineManager.Client != null)
                OnlineManager.Client.OnGameLivesChanged += OnLifeCountChanged;
        }

        public override string GetName() => "Life Count";

        public override string GetValue() => SelectedGame.Value.Lives.ToString();

        private void OnLifeCountChanged(object sender, LivesChangedEventArgs e) => NeedsStateUpdate = true;

        public override void Dispose()
        {
            if (OnlineManager.Client != null)
                OnlineManager.Client.OnGameLivesChanged -= OnLifeCountChanged;

            base.Dispose();
        }
    }
}