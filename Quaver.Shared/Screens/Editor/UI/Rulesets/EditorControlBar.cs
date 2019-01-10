using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets
{
    public class EditorControlBar : Sprite
    {
        /// <summary>
        /// </summary>
        private JukeboxButton ButtonPlayTest { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxButton ButtonPlaybackRate { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxButton ButtonBeatSnap { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxButton ButtonStopTrack { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxButton ButtonPauseTrack { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxButton ButtonPlayTrack { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap TextAudioTime { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorControlBar()
        {
            Size = new ScalableVector2(70, WindowManager.Height);
            Tint = Color.Black;
            Alpha = 0.70f;

            CreateAudioControlButtons();

            // Top Border Line
            // ReSharper disable once ObjectCreationAsStatement
            AddBorder(Color.White, 1);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // TextAudioTime.Text = TimeSpan.FromMilliseconds(AudioEngine.Track.Time).ToString(@"mm\:ss\.fff");
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateTextAudioTime() => TextAudioTime = new SpriteTextBitmap(FontsBitmap.AllerRegular, "00:00.000")
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = 15,
            Y = 2,
            FontSize = 20
        };

        private void CreateAudioControlButtons()
        {
            ButtonPlayTest = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_play_sign))
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(30, 30),
                Y = -15
            };

            ButtonPlayTest.Clicked += (o, e) => NotificationManager.Show(NotificationLevel.Warning, "Play Testing is not implemented yet!");

            ButtonPlaybackRate = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_time))
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(30, 30),
                Y = ButtonPlayTest.Y - ButtonPlayTest.Height - 20
            };

            ButtonPlaybackRate.Clicked += (o, e) => NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet");

            ButtonBeatSnap = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_align_justify))
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(30, 30),
                Y = ButtonPlaybackRate.Y - ButtonPlaybackRate.Height - 20
            };

            ButtonBeatSnap.Clicked += (o, e) => NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet");

            ButtonStopTrack = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_square_shape_shadow))
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(30, 30),
                Y = ButtonBeatSnap.Y - ButtonBeatSnap.Height - 20
            };

            ButtonStopTrack.Clicked += (o, e) => EditorScreen.StopTrack();

            ButtonPauseTrack = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_pause_symbol))
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(30, 30),
                Y = ButtonStopTrack.Y - ButtonStopTrack.Height - 20
            };

            ButtonPauseTrack.Clicked += (o, e) => EditorScreen.PauseTrack();

            ButtonPlayTrack = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_play_button))
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(30, 30),
                Y = ButtonPauseTrack.Y - ButtonPauseTrack.Height - 20
            };

            ButtonPlayTrack.Clicked += (o, e) => EditorScreen.PlayOrReplayTrack();
        }
    }
}