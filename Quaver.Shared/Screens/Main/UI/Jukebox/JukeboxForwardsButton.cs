using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.Music.Components;
using Wobble.Assets;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Main.UI.Jukebox
{
    public class JukeboxForwardsButton: IconButton
    {
        public JukeboxForwardsButton(IJukebox jukebox) : base(UserInterface.JukeboxBackwardButton,
            (o, e) =>
            {
                if (!OnlineManager.IsListeningPartyHost)
                {
                    NotificationManager.Show(NotificationLevel.Error, "You are not the host of listening party!");
                    return;
                }
                
                jukebox?.SelectNextTrack(Direction.Forward);
            })
        {
            SpriteEffect = SpriteEffects.FlipHorizontally;
        }
    }
}