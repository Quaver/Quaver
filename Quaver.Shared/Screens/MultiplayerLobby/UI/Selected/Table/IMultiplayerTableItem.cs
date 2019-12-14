using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;
using Wobble.Graphics;

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

        /// <summary>
        ///     The drawable used to select the item. This could be a checkbox/dropdown/etc.
        /// </summary>
        Drawable Selector { get; set; }

        /// <summary>
        ///     Updates the state of <see cref="Selector"/>
        /// </summary>
        void UpdateSelectorState();
    }
}