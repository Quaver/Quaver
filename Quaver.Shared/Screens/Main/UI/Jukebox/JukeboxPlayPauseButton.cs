using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Assets;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Main.UI.Jukebox
{
    public class JukeboxPlayPauseButton : IconButton
    {
        /// <summary>
        /// </summary>
        private FooterJukebox FooterJukebox { get; }

        /// <summary>
        /// </summary>
        private Texture2D PlayButton => UserInterface.JukeboxPlayButton;

        /// <summary>
        /// </summary>
        private Texture2D PauseButton => UserInterface.JukeboxPauseButton;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public JukeboxPlayPauseButton(FooterJukebox jukebox) : base(UserInterface.BlankBox, (sender, args) =>
        {
            if (AudioEngine.Track == null)
                return;

            lock (AudioEngine.Track)
            {
                if (AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Pause();
                else if (AudioEngine.Track.IsPaused)
                    AudioEngine.Track.Play();
                else if (AudioEngine.Track.IsStopped || AudioEngine.Track.IsDisposed)
                {
                    AudioEngine.LoadCurrentTrack();
                    AudioEngine.Track?.Play();
                }
            }
        })
        {
            FooterJukebox = jukebox;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            SetImage();
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void SetImage()
        {
            if (AudioEngine.Track != null && AudioEngine.Track.IsPlaying || FooterJukebox.LoadingNextTrack)
            {
                if (Image != PauseButton)
                    Image = PauseButton;
            }
            else if (Image != PlayButton)
                Image = PlayButton;
        }
    }
}