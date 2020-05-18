using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected
{
    public interface IMultiplayerGameComponent
    {
        /// <summary>
        /// </summary>
        Bindable<MultiplayerGame> SelectedGame { get; set; }

        /// <summary>
        ///     Updates the state of the component
        /// </summary>
        void UpdateState();
    }
}