using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Client.Objects.Multiplayer;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemAllowedGameModes : MultiplayerTableItem
    {
        public MultiplayerTableItemAllowedGameModes(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
        }

        public override string GetName() => MultiplayerLobbyLocalization.Get("AllowedGameModes");

        public override string GetValue()
        {
            if (SelectedGame.Value.AllowedGameModes == null)
                return MultiplayerLobbyLocalization.Get("None");

            var modesList = SelectedGame.Value.AllowedGameModes.Select(x => ModeHelper.ToShortHand((GameMode) x)).ToList();

            if (modesList.Count == 0)
                return MultiplayerLobbyLocalization.Get("None");

            if (modesList.Count == 1)
                return modesList.First();

            return string.Join(", ", modesList);
        }
    }
}
