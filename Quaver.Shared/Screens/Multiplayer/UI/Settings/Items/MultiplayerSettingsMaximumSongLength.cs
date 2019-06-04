using System;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsMaximumSongLength : MultiplayerSettingsText
    {
        public MultiplayerSettingsMaximumSongLength(string name, string value) : base(name, value)
        {
            CreateDialog = () =>
            {
                NotificationManager.Show(NotificationLevel.Warning, "Use the \"!mp maxlength\" command to change this setting!");
                return null;
            };

            OnlineManager.Client.OnMaxSongLengthChanged += OnMaxSongLengthChanged;
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnMaxSongLengthChanged -= OnMaxSongLengthChanged;
            base.Destroy();
        }

        private void OnMaxSongLengthChanged(object sender, MaxSongLengthChangedEventArgs e)
            => Value.Text = ToString(e.Seconds);

        public static string ToString(int seconds)
        {
            if (seconds >= 5000)
                return "Any";

            var t = TimeSpan.FromSeconds(seconds);
            return $"{t.Hours:00}:{t.Minutes:00}:{t.Seconds:00}";
        }
    }
}