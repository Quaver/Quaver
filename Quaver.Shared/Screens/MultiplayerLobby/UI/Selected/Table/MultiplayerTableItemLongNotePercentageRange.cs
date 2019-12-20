using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Online;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemLongNotePercentageRange : MultiplayerTableItem
    {
        public MultiplayerTableItemLongNotePercentageRange(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
            if (OnlineManager.Client != null)
                OnlineManager.Client.OnGameLongNotePercentageChanged += OnLongNotePercentageRangeChanged;
        }

        public override string GetName() => "Long Note % Range";

        public override string GetValue()
        {
            if (SelectedGame.Value.MinimumLongNotePercentage == 0 && SelectedGame.Value.MaximumLongNotePercentage == 100)
                return "Any";

            return $"{SelectedGame.Value.MinimumLongNotePercentage}-{SelectedGame.Value.MaximumLongNotePercentage}%";
        }

        private void OnLongNotePercentageRangeChanged(object sender, LongNotePercentageChangedEventArgs e) =>
            NeedsStateUpdate = true;

        public override void Dispose()
        {
            if (OnlineManager.Client != null)
                OnlineManager.Client.OnGameLongNotePercentageChanged -= OnLongNotePercentageRangeChanged;

            base.Dispose();
        }
    }
}