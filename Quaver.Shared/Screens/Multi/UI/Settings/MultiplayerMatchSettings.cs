using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Selected;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Multi.UI.Settings
{
    public class MultiplayerMatchSettings : SelectedGamePanel
    {
        public MultiplayerMatchSettings(Bindable<MultiplayerGame> game) : base(game, true)
        {
        }
    }
}