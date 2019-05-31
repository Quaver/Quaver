using Quaver.Server.Client.Handlers;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsLives : MultiplayerSettingsText
    {
        public MultiplayerSettingsLives(string name, string value) : base(name, value)
        {
            OnlineManager.Client.OnGameLivesChanged += OnGameLivesChanged;
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnGameLivesChanged -= OnGameLivesChanged;
            base.Destroy();
        }

        private void OnGameLivesChanged(object sender, LivesChangedEventArgs e)
            => Value.Text = e.Lives.ToString();
    }
}