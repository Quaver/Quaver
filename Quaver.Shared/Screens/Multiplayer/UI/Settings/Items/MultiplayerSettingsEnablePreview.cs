using System;
using System.Collections.Generic;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Multiplayer.UI.Dialogs;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsEnablePreview : MultiplayerSettingsText
    {
        public MultiplayerSettingsEnablePreview(string name, string value) : base(name, value)
        {
            CreateDialog = () =>
            {
                OnlineManager.Client?.ChangeGameEnablePreview(!OnlineManager.CurrentGame.EnablePreview);
                return null;
            };

            OnlineManager.Client.OnGameEnablePreviewChanged += OnGameEnablePreviewChanged;
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnGameEnablePreviewChanged -= OnGameEnablePreviewChanged;
            base.Destroy();
        }

        private void OnGameEnablePreviewChanged(object sender, EnablePreviewChangedEventArgs e)
        {
            Value.Text = BooleanToYesOrNo(e.EnablePreview);
        }
    }
}