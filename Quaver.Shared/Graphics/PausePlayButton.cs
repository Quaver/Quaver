using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Screens.Menu.UI.Jukebox;

namespace Quaver.Shared.Graphics
{
    public class PausePlayButton : JukeboxButton
    {
        public PausePlayButton() : base(FontAwesome.Get(FontAwesomeIcon.fa_pause_symbol))
        {
            Clicked += (sender, args) =>
            {
                if (AudioEngine.Track == null)
                    return;

                if (AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Pause();
                else if (AudioEngine.Track.IsPaused)
                    AudioEngine.Track.Play();
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (AudioEngine.Track != null)
            {
                if (AudioEngine.Track.IsPlaying)
                {
                    var pause = FontAwesome.Get(FontAwesomeIcon.fa_pause_symbol);

                    if (Image != pause)
                        Image = pause;
                }
                else if (AudioEngine.Track.IsStopped || AudioEngine.Track.IsPaused)
                {
                    var play = FontAwesome.Get(FontAwesomeIcon.fa_play_button);

                    if (Image != play)
                        Image = play;
                }
            }

            base.Update(gameTime);
        }
    }
}
