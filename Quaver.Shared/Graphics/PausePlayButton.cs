using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Audio.Tracks;

namespace Quaver.Shared.Graphics
{
    public class PausePlayButton : IconButton
    {
        private Texture2D PauseButton { get; }

        private Texture2D PlayButton { get; }

        /// <summary>
        /// </summary>
        private IAudioTrack Track { get; }

        public PausePlayButton(Texture2D pauseButton = null, Texture2D playButton = null, IAudioTrack track = null)
            : base(pauseButton ?? FontAwesome.Get(FontAwesomeIcon.fa_pause_symbol))
        {
            Track = track ?? AudioEngine.Track;

            PauseButton = pauseButton ?? FontAwesome.Get(FontAwesomeIcon.fa_pause_symbol);
            PlayButton = playButton ?? FontAwesome.Get(FontAwesomeIcon.fa_play_button);

            Clicked += (sender, args) =>
            {
                if (Track == null || Track.IsDisposed)
                    return;

                if (Track.IsPlaying)
                    Track.Pause();
                else
                    Track.Play();
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Track != null)
            {
                if (Track.IsPlaying)
                {
                    var pause = PauseButton;

                    if (Image != pause)
                        Image = pause;
                }
                else if (Track.IsStopped || Track.IsPaused)
                {
                    var play = PlayButton;

                    if (Image != play)
                        Image = play;
                }
            }

            base.Update(gameTime);
        }
    }
}
