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
        }

        public override string GetName() => "In Progress";

        public override string GetValue() => SelectedGame.Value.InProgress ? "Yes" : "No";
    }
}