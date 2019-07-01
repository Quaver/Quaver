using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Multiplayer.UI.Dialogs;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsHealthType : MultiplayerSettingsText
    {
        public MultiplayerSettingsHealthType(string name, string value) : base(name, value, CreateMenuDialog)
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

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static MenuDialog CreateMenuDialog()
        {
            var options = new List<IMenuDialogOption>()
            {
                new MenuDialogOption("Manual Regeneration - Revive After Dying", () =>
                {
                    if (OnlineManager.CurrentGame.HealthType == (byte) MultiplayerHealthType.Manual_Regeneration)
                        return;

                    OnlineManager.Client?.ChangeGameHealthType(MultiplayerHealthType.Manual_Regeneration);
                }),
                new MenuDialogOption("Lives - Die & Lose a Life", () =>
                {
                    if (OnlineManager.CurrentGame.HealthType == (byte) MultiplayerHealthType.Lives)
                        return;

                    OnlineManager.Client?.ChangeGameHealthType(MultiplayerHealthType.Lives);
                }),
                new MenuDialogOption("Close", () => { })
            };

            return new MenuDialogMultiplayer("Change Health Type", options);
        }
    }
}