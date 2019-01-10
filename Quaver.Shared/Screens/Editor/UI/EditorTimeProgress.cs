using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Gameplay.UI;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Screens.Editor.UI
{
    public class EditorTimeProgress : Sprite
    {
        /// <summary>
        /// </summary>
        private EditorScreenView View { get; }

        /// <summary>
        /// </summary>
        private Qua Map { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap TextAudioTime { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap TextAudioTimeLeft { get; set; }

        /// <summary>
        /// </summary>
        private ProgressBar TimeProgressBar { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton DraggableProgressButton { get; set; }

        /// <summary>
        ///     Keeps track of if the user was previously dragging in the last frame.
        ///     Lets us know if they've stopped holding down, so we can resume the audio track.
        /// </summary>
        private bool DraggingInLastFrame { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        /// <param name="map"></param>
        public EditorTimeProgress(EditorScreenView view, Qua map)
        {
            View = view;
            Map = map;
            Size = new ScalableVector2(WindowManager.Width - View.ControlBar.Width, 50);
            Tint = Color.Black;
            Alpha = 0.75f;

            // CreateBorderLines();
            CreateTextAudioTime();
            CreateTimeProgressBar();
            CreateDraggableProgressButton();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            TextAudioTime.Text = TimeSpan.FromMilliseconds(AudioEngine.Track.Time).ToString(@"mm\:ss\.fff");

            if (!AudioEngine.Track.IsDisposed)
                TextAudioTimeLeft.Text = "-" + TimeSpan.FromMilliseconds(AudioEngine.Track.Length - AudioEngine.Track.Time).ToString(@"mm\:ss\.fff");
            else
                TextAudioTimeLeft.Text = "00:00.000";

            TimeProgressBar.Bindable.Value = AudioEngine.Track.Time;

            if (DraggableProgressButton.IsHeld && !AudioEngine.Track.IsDisposed)
            {
                var percentage = (MouseManager.CurrentState.X - DraggableProgressButton.AbsolutePosition.X) / DraggableProgressButton.AbsoluteSize.X;

                var targetPos = percentage * AudioEngine.Track.Length;

                if ((int) targetPos != (int) AudioEngine.Track.Time && targetPos >= 0 && targetPos <= AudioEngine.Track.Length)
                {
                    if (AudioEngine.Track.IsPlaying)
                        AudioEngine.Track.Pause();

                    AudioEngine.Track.Seek(targetPos);

                    var screen = (EditorScreen) View.Screen;
                    screen.SetHitSoundObjectIndex();
                }

                DraggingInLastFrame = true;
            }
            else if (DraggingInLastFrame)
            {
                if (AudioEngine.Track.IsPaused || (AudioEngine.Track.IsStopped && !AudioEngine.Track.HasPlayed))
                    AudioEngine.Track.Play();

                DraggingInLastFrame = false;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateTextAudioTime()
        {
            TextAudioTime = new SpriteTextBitmap(FontsBitmap.AllerRegular, "00:00.000")
            {
                Parent = this,
                X = 16,
                FontSize = 20,
                Alignment = Alignment.MidLeft
            };

            TextAudioTimeLeft = new SpriteTextBitmap(FontsBitmap.AllerRegular, "-00:00.000")
            {
                Parent = this,
                X = Width - TextAudioTime.X,
                FontSize = 20,
                Alignment = Alignment.MidLeft
            };

            TextAudioTimeLeft.X -= TextAudioTimeLeft.Width;
        }

        /// <summary>
        /// </summary>
        private void CreateTimeProgressBar() => TimeProgressBar = new ProgressBar(
            new Vector2(TextAudioTimeLeft.X - TextAudioTime.X - TextAudioTimeLeft.Width - 20, 3), 0, AudioEngine.Track.Length,
            0, Color.LightGray, Colors.MainAccent)
        {
            Parent = this,
            X = TextAudioTime.X + TextAudioTime.Width + 15,
            Alignment = Alignment.MidLeft,
            Y = 2
        };

        /// <summary>
        /// </summary>
        private void CreateDraggableProgressButton() => DraggableProgressButton = new ImageButton(UserInterface.BlankBox)
        {
            Parent = this,
            Size = new ScalableVector2(TimeProgressBar.Width, Height),
            X = TimeProgressBar.X,
            Alpha = 0
        };
    }
}