using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemDifficultyRange : MultiplayerTableItem
    {
        public MultiplayerTableItemDifficultyRange(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
            if (OnlineManager.Client != null)
                OnlineManager.Client.OnDifficultyRangeChanged += OnDifficultyRangeChanged;
        }

        public override string GetName() => "Difficulty Range";

        public override string GetValue()
        {
            // ReSharper disable twice CompareOfFloatsByEqualityOperator
            if (SelectedGame.Value.MinimumDifficultyRating == 0 && SelectedGame.Value.MaximumDifficultyRating == 9999)
                return "Any";

            return $"{StringHelper.RatingToString(SelectedGame.Value.MinimumDifficultyRating)} " +
                   $"- {StringHelper.RatingToString(SelectedGame.Value.MaximumDifficultyRating)}";
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDifficultyRangeChanged(object sender, DifficultyRangeChangedEventArgs e) => NeedsStateUpdate = true;

        public override void Dispose()
        {
            if (OnlineManager.Client != null)
                OnlineManager.Client.OnDifficultyRangeChanged -= OnDifficultyRangeChanged;

            base.Dispose();
        }
    }
}