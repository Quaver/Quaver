using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemAutoHostRotation : MultiplayerTableItem
    {
        public MultiplayerTableItemAutoHostRotation(Bindable<MultiplayerGame> game) : base(game)
        {
        }

        public override string GetName() => "Auto Host Rotation";

        public override string GetValue() => SelectedGame.Value.HostRotation ? "Yes" : "No";
    }
}