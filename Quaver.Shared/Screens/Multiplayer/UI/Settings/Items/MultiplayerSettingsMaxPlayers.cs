using Quaver.Server.Client.Handlers;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsMaxPlayers : MultiplayerSettingsText
    {
        public MultiplayerSettingsMaxPlayers(string name, string value) : base(name, value)
        {
            OnlineManager.Client.OnGameMaxPlayersChanged += OnGameMaxPlayersChanged;
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnGameMaxPlayersChanged -= OnGameMaxPlayersChanged;
            base.Destroy();
        }

        private void OnGameMaxPlayersChanged(object sender, MaxPlayersChangedEventArgs e)
            => Value.Text = e.MaxPlayers.ToString();
    }
}