using System;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Multi.UI.Dialogs
{
    public sealed class ConfirmReady : YesNoDialog
    {
        public ConfirmReady() : base("CONFIRM READY",
            "Readying up as the host will start the round, are you\nsure you would like to start without all players ready?")
        {
            YesAction += () =>
            {
                OnlineManager.Client?.MultiplayerGameIsReady();
                OnlineManager.Client?.MultiplayerGameStartCountdown();
            };
        }
    }
}