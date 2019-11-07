using System;
using Quaver.Shared.Online;

namespace Quaver.Shared.Graphics.Overlays.Hub.SongRequests.Header
{
    public class UnlinkTwitchConfirmationDialog : YesNoDialog
    {
        public UnlinkTwitchConfirmationDialog() : base($"UNLINK TWITCH ACCOUNT",
            $"Are you sure you would like to unlink your Twitch account?\nUsername: \"{OnlineManager.TwitchUsername}\"",
            () => OnlineManager.Client?.UnlinkTwitchAccount())
        {
        }
    }
}