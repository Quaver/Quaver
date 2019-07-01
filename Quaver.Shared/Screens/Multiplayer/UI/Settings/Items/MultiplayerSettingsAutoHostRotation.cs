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
        public MultiplayerSettingsAutoHostRotation(string name, string value) : base(name, value)
        {
            CreateDialog = () =>
            {
                OnlineManager.Client?.ChangeGameAutoHostRotation(!OnlineManager.CurrentGame.HostRotation);
                return null;
            };

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
    }
}