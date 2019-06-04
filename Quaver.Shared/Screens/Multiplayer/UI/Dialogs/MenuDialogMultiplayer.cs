using System.Collections.Generic;
using Quaver.Server.Client;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Multiplayer.UI.Dialogs
{
    public class MenuDialogMultiplayer : MenuDialog
    {
        public MenuDialogMultiplayer(string name, List<IMenuDialogOption> options) : base(name, options)
        {
            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        }

        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;

            base.Destroy();
        }

        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            if (e.Value != ConnectionStatus.Connected)
                DialogManager.Dismiss(this);
        }
    }
}