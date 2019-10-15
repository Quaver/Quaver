using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.Music.Components;
using Wobble.Assets;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Main.UI.Jukebox
{
    public class JukeboxBackwardsButton : IconButton
    {
        public JukeboxBackwardsButton(IJukebox jukebox) : base(UserInterface.JukeboxBackwardButton,(o, e) =>
            {
                if (AudioEngine.Track == null)
                    return;

                lock (AudioEngine.Track)
                {
                    // Restart the track if the user has pressed the button after 2 seconds of the track playing
                    if (AudioEngine.Track.Position >= 2000 || jukebox.TrackListQueuePosition == 0)
                    {
                        AudioEngine.LoadCurrentTrack();
                        AudioEngine.Track.Play();

                        return;
                    }

                    // Otherwise go back in the jukebox's queue if applicable
                    jukebox.SelectNextTrack(Direction.Backward);
                }
            })
        {
        }
    }
}