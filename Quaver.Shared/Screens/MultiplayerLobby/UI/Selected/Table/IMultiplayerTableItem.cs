using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public interface IMultiplayerTableItem
    {
        /// <summary>
        /// </summary>
        Bindable<MultiplayerGame> SelectedGame { get; set; }

        /// <summary>
        ///     Retrieves the name of the table item
        /// </summary>
        /// <returns></returns>
        string GetName();

        /// <summary>
        ///     Retrieves the value of the table item
        /// </summary>
        /// <returns></returns>
        string GetValue();
    }
}