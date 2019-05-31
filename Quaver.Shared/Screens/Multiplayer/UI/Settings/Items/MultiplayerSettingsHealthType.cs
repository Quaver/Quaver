using Quaver.API.Enums;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsHealthType : MultiplayerSettingsText
    {
        public MultiplayerSettingsHealthType(string name, string value) : base(name, value)
        {
            OnlineManager.Client.OnGameHealthTypeChanged += OnGameHealthTypeChanged;
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnGameHealthTypeChanged -= OnGameHealthTypeChanged;
            base.Destroy();
        }

        private void OnGameHealthTypeChanged(object sender, HealthTypeChangedEventArgs e)
            => Value.Text = ((MultiplayerHealthType) e.HealthType).ToString().Replace("_", " ");
    }
}