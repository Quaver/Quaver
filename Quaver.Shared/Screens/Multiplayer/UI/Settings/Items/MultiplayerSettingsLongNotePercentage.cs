using Quaver.Server.Client.Handlers;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsLongNotePercentage : MultiplayerSettingsText
    {
        public MultiplayerSettingsLongNotePercentage(string name, string value) : base(name, value)
        {
            CreateDialog = () =>
            {
                NotificationManager.Show(NotificationLevel.Warning, "Use the \"!mp lnmin\" or \"!mp lnmax\" command to change this setting!");
                return null;
            };

            OnlineManager.Client.OnGameLongNotePercentageChanged += OnGameLongNotePercentageChanged;
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnGameLongNotePercentageChanged -= OnGameLongNotePercentageChanged;
            base.Destroy();
        }

        private void OnGameLongNotePercentageChanged(object sender, LongNotePercentageChangedEventArgs e)
            => Value.Text = ToString(e.Minimum, e.Maximum);

        public static string ToString(int min, int max)
            => $"{min}-{max}%";
    }
}