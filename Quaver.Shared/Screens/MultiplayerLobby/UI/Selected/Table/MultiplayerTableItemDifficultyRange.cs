using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Helpers;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemDifficultyRange : MultiplayerTableItem
    {
        public MultiplayerTableItemDifficultyRange(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
        }

        public override string GetName() => "Difficulty Range";

        public override string GetValue()
        {
            return $"{StringHelper.RatingToString(SelectedGame.Value.MinimumDifficultyRating)} " +
                   $"- {StringHelper.RatingToString(SelectedGame.Value.MaximumDifficultyRating)}";
        }
    }
}