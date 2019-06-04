using System.Collections.Generic;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Multiplayer.UI.Dialogs;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsMaxPlayers : MultiplayerSettingsText
    {
        public MultiplayerSettingsMaxPlayers(string name, string value) : base(name, value, CreateMenuDialog)
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

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static MenuDialog CreateMenuDialog()
        {
            var options = new List<IMenuDialogOption>();

            for (var i = OnlineManager.CurrentGame.PlayerIds.Count; i < 16; i++)
            {
                var num = i + 1;

                options.Add(new MenuDialogOption($"{i + 1}", () =>
                {
                    if (OnlineManager.CurrentGame.PlayerIds.Count > num)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "There are more players in the game than the max players you want to set!");
                        return;
                    }

                    OnlineManager.Client?.ChangeGameMaxPlayers(num);
                }));
            }

            options.Add(new MenuDialogOption("Close", () => { }));
            return new MenuDialogMultiplayer("Change Max Player Count", options);
        }
    }
}