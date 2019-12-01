using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemAllowedGameModes : MultiplayerTableItem
    {
        public MultiplayerTableItemAllowedGameModes(Bindable<MultiplayerGame> game) : base(game)
        {
        }

        public override string GetName() => "Allowed Game Modes";

        public override string GetValue()
        {
            if (SelectedGame.Value.AllowedGameModes == null)
                return "None";

            var modesList = SelectedGame.Value.AllowedGameModes.Select(x => ModeHelper.ToShortHand((GameMode) x)).ToList();

            if (modesList.Count == 0)
                return "None";

            if (modesList.Count == 1)
                return modesList.First();

            return string.Join(", ", modesList);
        }
    }
}