using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Multiplayer.UI.Dialogs;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsLives : MultiplayerSettingsText
    {
        public MultiplayerSettingsLives(string name, string value) : base(name, value, CreateMenuDialog)
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

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static MenuDialog CreateMenuDialog()
        {
            if (OnlineManager.CurrentGame.HealthType != (byte) MultiplayerHealthType.Lives)
            {
                NotificationManager.Show(NotificationLevel.Error, "You can only change the life count if the health type is set to Lives!");
                return null;
            }

            var options = new List<IMenuDialogOption>();

            for (var i = 0; i < 10; i++)
            {
                var num = i + 1;

                options.Add(new MenuDialogOption($"{i + 1}", () =>
                {
                    OnlineManager.Client?.ChangeGameLivesCount(num);
                }));
            }

            options.Add(new MenuDialogOption("Close", () => { }));
            return new MenuDialogMultiplayer("Change Life Count", options);
        }
    }
}