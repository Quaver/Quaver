using System;
using System.Collections.Generic;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Multiplayer.UI.Dialogs;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsAutoHostRotation : MultiplayerSettingsText
    {
        public MultiplayerSettingsAutoHostRotation(string name, string value) : base(name, value, CreateMenuDialog)
        {
            OnlineManager.Client.OnGameHostRotationChanged += OnGameHostRotationChanged;
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnGameHostRotationChanged -= OnGameHostRotationChanged;
            base.Destroy();
        }

        private void OnGameHostRotationChanged(object sender, HostRotationChangedEventArgs e)
        {
            Value.Text = BooleanToYesOrNo(e.HostRotation);
        }

        private static MenuDialog CreateMenuDialog()
        {
            var options = new List<IMenuDialogOption>()
            {
                new MenuDialogOption("Yes", () =>
                {
                    if (OnlineManager.CurrentGame.HostRotation)
                        return;

                    OnlineManager.Client?.ChangeGameAutoHostRotation(true);
                }),
                new MenuDialogOption("No", () =>
                {
                    if (!OnlineManager.CurrentGame.HostRotation)
                        return;

                    OnlineManager.Client?.ChangeGameAutoHostRotation(false);
                })
            };

            return new MenuDialogMultiplayer("Auto Host Rotation", options);
        }
    }
}