using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemInProgress : MultiplayerTableItem
    {
        public MultiplayerTableItemInProgress(Bindable<MultiplayerGame> game) : base(game)
        {
        }

        public override string GetName() => "In Progress";

        public override string GetValue() => SelectedGame.Value.InProgress ? "Yes" : "No";
    }
}