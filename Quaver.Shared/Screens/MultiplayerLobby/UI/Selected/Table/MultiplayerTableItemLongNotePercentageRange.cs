using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemLongNotePercentageRange : MultiplayerTableItem
    {
        public MultiplayerTableItemLongNotePercentageRange(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
        }

        public override string GetName() => "Long Note % Range";

        public override string GetValue()
        {
            return $"{SelectedGame.Value.MinimumLongNotePercentage}-{SelectedGame.Value.MaximumLongNotePercentage}%";
        }
    }
}