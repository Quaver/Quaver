using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Editor.UI.Rulesets;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Editor.UI
{
    public class EditorControlBar : Sprite
    {
        /// <summary>
        /// </summary>
        private EditorControlButton ButtonPlayTest { get; set; }

        /// <summary>
        /// </summary>
        private EditorControlButton ButtonPlaybackRate { get; set; }

        /// <summary>
        /// </summary>
        private EditorControlButton ButtonBeatSnap { get; set; }

        /// <summary>
        /// </summary>
        private EditorControlButton ButtonStopTrack { get; set; }

        /// <summary>
        /// </summary>
        private EditorControlButton ButtonPlayPauseTrack { get; set; }

        /// <summary>
        /// </summary>
        private EditorControlButton ButtonRestartTrack { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorControlBar()
        {
            Size = new ScalableVector2(70, WindowManager.Height);
            Tint = Color.Black;
            Alpha = 0.75f;

            CreateAudioControlButtons();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            ButtonPlayPauseTrack.Image =  FontAwesome.Get(AudioEngine.Track.IsPlaying ? FontAwesomeIcon.fa_pause_symbol : FontAwesomeIcon.fa_play_button);
            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates generic buttons used to control audio/misc.
        /// </summary>
        private void CreateAudioControlButtons()
        {
            ButtonPlayTest = new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_play_sign), "Test Play", 60)
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(30, 30),
                Y = -15
            };

            ButtonPlayTest.Clicked += (o, e) => NotificationManager.Show(NotificationLevel.Warning, "Play Testing is not implemented yet!");

            ButtonPlaybackRate = new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_time), "Change Playback Rate", 60)
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(30, 30),
                Y = ButtonPlayTest.Y - ButtonPlayTest.Height - 20
            };

            ButtonPlaybackRate.Clicked += (o, e) => NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet");

            ButtonBeatSnap = new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_align_justify), "Change Beat Snap", 60)
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(30, 30),
                Y = ButtonPlaybackRate.Y - ButtonPlaybackRate.Height - 20
            };

            ButtonBeatSnap.Clicked += (o, e) => NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet");

            ButtonStopTrack = new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_square_shape_shadow), "Stop Track", 60)
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(30, 30),
                Y = ButtonBeatSnap.Y - ButtonBeatSnap.Height - 20
            };

            ButtonStopTrack.Clicked += (o, e) => EditorScreen.StopTrack();

            ButtonPlayPauseTrack = new EditorControlButton(FontAwesome.Get(AudioEngine.Track.IsPlaying
                ? FontAwesomeIcon.fa_pause_symbol : FontAwesomeIcon.fa_play_button), "Play/Pause Track", 60)
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(30, 30),
                Y = ButtonStopTrack.Y - ButtonStopTrack.Height - 20
            };

            ButtonPlayPauseTrack.Clicked += (o, e) =>
            {
                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;
                screen?.PlayPauseTrack();
            };

            ButtonRestartTrack = new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_undo_arrow), "Restart Track", 60)
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(30, 30),
                Y = ButtonPlayPauseTrack.Y - ButtonPlayPauseTrack.Height - 20,
            };

            ButtonRestartTrack.Clicked += (o, e) =>
            {
                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;
                screen?.RestartTrack();
            };
        }
    }
}