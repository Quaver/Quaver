using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemFreeRate : MultiplayerTableItem
    {
        public MultiplayerTableItemFreeRate(Bindable<MultiplayerGame> game) : base(game)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string GetName() => "Free Rate";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string GetValue()
            => SelectedGame.Value.FreeModType.HasFlag(MultiplayerFreeModType.Rate) ? "Yes" : "No";
    }
}