using System;
using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemSongLength : MultiplayerTableItem
    {
        public MultiplayerTableItemSongLength(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
        }

        public override string GetName() => "Maximum Song Length";

        public override string GetValue()
        {
            var t = TimeSpan.FromSeconds(SelectedGame.Value.MaximumSongLength);

            return $"{t.Minutes}:{t.Seconds:00}";
        }
    }
}