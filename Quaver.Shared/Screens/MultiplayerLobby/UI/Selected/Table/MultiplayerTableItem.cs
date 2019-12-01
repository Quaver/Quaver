using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public abstract class MultiplayerTableItem : IMultiplayerTableItem, IMultiplayerGameComponent
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public Bindable<MultiplayerGame> SelectedGame { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        public MultiplayerTableItem(Bindable<MultiplayerGame> game) => SelectedGame = game;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public virtual string GetName() => "";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public virtual string GetValue() => "";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public virtual void UpdateState()
        {
        }
    }
}