using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemPlayers : MultiplayerTableItem
    {
        public MultiplayerTableItemPlayers(Bindable<MultiplayerGame> game) : base(game)
        {
        }

        public override string GetName() => "Players";

        public override string GetValue() => $"{SelectedGame.Value.PlayerIds.Count}/{SelectedGame.Value.MaxPlayers}";
    }
}