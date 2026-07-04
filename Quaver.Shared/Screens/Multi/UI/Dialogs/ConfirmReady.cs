using System;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Multi.UI.Dialogs
{
    public sealed class ConfirmReady : YesNoDialog
    {
        public ConfirmReady() : base(MultiLocalization.Get("ConfirmReadyTitle"),
            MultiLocalization.Get("ConfirmReadyMessage"))
        {
            YesAction += () =>
            {
                OnlineManager.Client?.MultiplayerGameIsReady();
                OnlineManager.Client?.MultiplayerGameStartCountdown();
            };
        }
    }
}
