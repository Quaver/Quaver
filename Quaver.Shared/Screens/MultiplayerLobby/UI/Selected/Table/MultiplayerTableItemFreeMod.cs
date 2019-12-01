using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemFreeMod : MultiplayerTableItem
    {
        public MultiplayerTableItemFreeMod(Bindable<MultiplayerGame> game) : base(game)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string GetName() => "Free Mod";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string GetValue()
            => SelectedGame.Value.FreeModType.HasFlag(MultiplayerFreeModType.Regular) ? "Yes" : "No";
    }
}