using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsAllowedGameModes : MultiplayerSettingsText
    {
        public MultiplayerSettingsAllowedGameModes(string name, string value) : base(name, value)
        {
            OnlineManager.Client.OnAllowedModesChanged += OnAllowedModesChanged;

            CreateDialog = () =>
            {
                NotificationManager.Show(NotificationLevel.Warning, "Use the \"!mp allowmode\" or \"!mp disallowmode\" command to change this setting!");
                return null;
            };
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnAllowedModesChanged -= OnAllowedModesChanged;
            base.Destroy();
        }

        private void OnAllowedModesChanged(object sender, AllowedModesChangedEventArgs e)
            => Value.Text = AllowedModesToString(e.Modes);

        public static string AllowedModesToString(List<byte> modes)
        {
            var modesList = modes.Select(x => ModeHelper.ToShortHand((GameMode) x)).ToList();

            if (modesList.Count == 0)
                return "None";

            if (modesList.Count == 1)
                return modesList.First();

            return string.Join(", ", modesList);
        }
    }
}