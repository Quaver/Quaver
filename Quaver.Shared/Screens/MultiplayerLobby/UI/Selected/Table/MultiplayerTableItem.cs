using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;
using Wobble.Graphics;

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
        public bool IsMultiplayer { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public virtual Drawable Selector { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <param name="isMultiplayer"></param>
        public MultiplayerTableItem(Bindable<MultiplayerGame> game, bool isMultiplayer)
        {
            IsMultiplayer = isMultiplayer;
            SelectedGame = game;
        }

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

        /// <summary>
        /// </summary>
        public void UpdateSelectorState()
        {
        }
    }
}