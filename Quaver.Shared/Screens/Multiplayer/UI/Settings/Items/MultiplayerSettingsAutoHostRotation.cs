using System;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings.Items
{
    public class MultiplayerSettingsAutoHostRotation : MultiplayerSettingsText
    {
        public MultiplayerSettingsAutoHostRotation(string name, string value) : base(name, value)
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
            Console.WriteLine(e.HostRotation);
            Value.Text = BooleanToYesOrNo(e.HostRotation);
        }
    }
}