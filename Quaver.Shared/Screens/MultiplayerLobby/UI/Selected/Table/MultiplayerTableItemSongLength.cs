using System;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Online;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public class MultiplayerTableItemSongLength : MultiplayerTableItem
    {
        public MultiplayerTableItemSongLength(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
            if (OnlineManager.Client != null)
                OnlineManager.Client.OnMaxSongLengthChanged += OnSongLengthChanged;
        }

        public override string GetName() => "Maximum Song Length";

        public override string GetValue()
        {
            if (SelectedGame.Value.MaximumSongLength == 999999999)
                return "Any";

            var t = TimeSpan.FromSeconds(SelectedGame.Value.MaximumSongLength);

            return $"{t.Minutes}:{t.Seconds:00}";
        }

        private void OnSongLengthChanged(object sender, MaxSongLengthChangedEventArgs e) => NeedsStateUpdate = true;

        public override void Dispose()
        {
            if (OnlineManager.Client != null)
                OnlineManager.Client.OnMaxSongLengthChanged += OnSongLengthChanged;

            base.Dispose();
        }
    }
}