using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemLifeCount : MultiplayerTableItem
    {
        public MultiplayerTableItemLifeCount(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
        }

        public override string GetName() => "Life Count";

        public override string GetValue() => SelectedGame.Value.Lives.ToString();
    }
}