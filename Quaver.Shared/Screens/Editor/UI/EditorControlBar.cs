/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Quaver.Shared.Screens.Gameplay.UI;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Screens.Editor.UI
{
    public class EditorControlBar : Sprite
    {
        /// <summary>
        /// </summary>
        private EditorScreenView View { get; }

        /// <summary>
        /// </summary>
        private Qua Map { get; }

        /// <summary>
        /// </summary>
        private EditorControlButton ButtonPlayPauseTrack { get; set; }

        /// <summary>
        /// </summary>
        private EditorControlButton ButtonStopTrack { get; set; }

        /// <summary>
        /// </summary>
        private EditorControlButton ButtonRestartTrack { get; set; }

        /// <summary>
        /// </summary>
        private EditorControlButton ButtonPlayTest { get; set; }

        /// <summary>
        /// </summary>
        private EditorControlButton ButtonBeatSnap { get; set; }

        /// <summary>
        /// </summary>
        private EditorControlButton ButtonScrollDirection { get; set; }

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

        /// <summary>
        /// </summary>
        private bool WasPlayingBeforeDragging { get; set; }

        /// <summary>
        /// </summary>
        private Sprite ProgressBall { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        /// <param name="map"></param>
        public EditorControlBar(EditorScreenView view, Qua map)
        {
            View = view;
            Map = map;
            Size = new ScalableVector2(WindowManager.Width, 38);
            Tint = ColorHelper.HexToColor("#161616");
            Alpha = 1;

            CreateLeftSideButtons();
            CreateRightSideButtons();
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
            ButtonPlayPauseTrack.Image =  FontAwesome.Get(AudioEngine.Track.IsPlaying
                ? FontAwesomeIcon.fa_pause_symbol : FontAwesomeIcon.fa_play_button);

            UpdateAudioTimeText();
            TimeProgressBar.Bindable.Value = AudioEngine.Track.Time;
            HandleProgressBarDragging();

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateLeftSideButtons()
        {
            const int padding = 50;

            // Pause/Play
            ButtonPlayPauseTrack = new EditorControlButton(FontAwesome.Get(AudioEngine.Track.IsPlaying
                ? FontAwesomeIcon.fa_pause_symbol : FontAwesomeIcon.fa_play_button), "Play/Pause Track", padding)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(18,18),
                X = 20
            };

            ButtonPlayPauseTrack.Clicked += (o, e) =>
            {
                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;
                screen?.PlayPauseTrack();
            };

            // Stop
            ButtonStopTrack = new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_square_shape_shadow), "Stop Track", padding)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(18,18),
                X = ButtonPlayPauseTrack.X + ButtonPlayPauseTrack.Width + 15
            };

            ButtonStopTrack.Clicked += (o, e) => EditorScreen.StopTrack();

            // Restart
            ButtonRestartTrack = new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_refresh_page_option), "Restart Track", padding)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(18,18),
                X = ButtonStopTrack.X + ButtonStopTrack.Width + 15
            };

            ButtonRestartTrack.Clicked += (o, e) =>
            {
                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;
                screen?.RestartTrack();
            };
        }

        /// <summary>
        /// </summary>
        private void CreateRightSideButtons()
        {
            const int padding = 50;

            ButtonPlayTest = new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_play_sign), "Test Play", padding, Alignment.MidRight)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(18,18),
                X = -ButtonPlayPauseTrack.X,
            };

            ButtonPlayTest.Clicked += (o, e) =>
            {
                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;

                // ReSharper disable once SwitchStatementMissingSomeCases
                screen?.GoPlayTest();
            };

            ButtonBeatSnap = new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_align_justify), "Change Beat Snap", padding, Alignment.MidRight)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(18,18),
                X = ButtonPlayTest.X - ButtonPlayTest.Width - 20
            };

            ButtonBeatSnap.Clicked += (o, e) =>
            {
                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;

                // ReSharper disable once SwitchStatementMissingSomeCases
                screen?.ChangeBeatSnap(Direction.Forward);
            };

            ButtonScrollDirection = new EditorControlButton(FontAwesome.Get(FontAwesomeIcon.fa_exchange_arrows), "Change Scroll Direction",
                padding, Alignment.MidRight)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(18,18),
                X = ButtonBeatSnap.X - ButtonBeatSnap.Width - 20
            };

            ButtonScrollDirection.Clicked += (o, e) =>
            {
                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;
                var ruleset = screen?.Ruleset as EditorRulesetKeys;
                //cruleset?.ToggleScrollDirection();
                NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet");
            };
        }

        /// <summary>
        /// </summary>
        private void CreateTextAudioTime()
        {
            TextAudioTime = new SpriteTextBitmap(FontsBitmap.MuliBold, "00:00.000")
            {
                Parent = this,
                X = ButtonRestartTrack.X + ButtonRestartTrack.Width + 15,
                FontSize = 22,
                Y = -2,
                Alignment = Alignment.MidLeft
            };

            TextAudioTimeLeft = new SpriteTextBitmap(FontsBitmap.MuliBold, "-00:00.000")
            {
                Parent = this,
                Y = -2,
                X = ButtonScrollDirection.AbsolutePosition.X - ButtonScrollDirection.Width - 15,
                FontSize = 22,
                Alignment = Alignment.MidLeft
            };

            TextAudioTimeLeft.X -= TextAudioTimeLeft.Width / 2f + 25;
        }

        /// <summary>
        /// </summary>
        private void UpdateAudioTimeText()
        {
            TextAudioTime.Text = TimeSpan.FromMilliseconds(AudioEngine.Track.Time).ToString(@"mm\:ss\.fff");

            if (!AudioEngine.Track.IsDisposed)
                TextAudioTimeLeft.Text = "-" + TimeSpan.FromMilliseconds(AudioEngine.Track.Length - AudioEngine.Track.Time).ToString(@"mm\:ss\.fff");
            else
                TextAudioTimeLeft.Text = "00:00.000";
        }

        /// <summary>
        /// </summary>
        private void CreateTimeProgressBar()
        {
            TimeProgressBar = new ProgressBar(
                new Vector2(TextAudioTimeLeft.X - TextAudioTime.X - TextAudioTimeLeft.Width - 20, 3), 0,
                AudioEngine.Track.Length,
                0, Color.LightGray, Colors.MainAccent)
            {
                Parent = this,
                X = TextAudioTime.X + TextAudioTime.Width + 15,
                Alignment = Alignment.MidLeft,
                Y = 2
            };

            ProgressBall = new Sprite()
            {
                Parent = TimeProgressBar.ActiveBar,
                Size = new ScalableVector2(10, 10),
                Alignment = Alignment.MidRight,
                Tint = Color.White,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_circle)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDraggableProgressButton() => DraggableProgressButton = new ImageButton(UserInterface.BlankBox)
        {
            Parent = this,
            Size = new ScalableVector2(TimeProgressBar.Width, Height),
            X = TimeProgressBar.X,
            Alpha = 0
        };

        /// <summary>
        ///     Handles logic for when the user is dragging with the progress seek bar.
        /// </summary>
        private void HandleProgressBarDragging()
        {
            if (DraggableProgressButton.IsHeld && !AudioEngine.Track.IsDisposed)
            {
                var percentage = (MouseManager.CurrentState.X - DraggableProgressButton.AbsolutePosition.X) / DraggableProgressButton.AbsoluteSize.X;

                var targetPos = percentage * AudioEngine.Track.Length;

                if ((int) targetPos != (int) AudioEngine.Track.Time && targetPos >= 0 && targetPos <= AudioEngine.Track.Length)
                {
                    if (AudioEngine.Track.IsPlaying)
                    {
                        WasPlayingBeforeDragging = true;
                        AudioEngine.Track.Pause();
                    }

                    AudioEngine.Track.Seek(targetPos);

                    var screen = (EditorScreen) View.Screen;
                    screen.SetHitSoundObjectIndex();

                    if (screen.Ruleset is EditorRulesetKeys ruleset)
                        ruleset.ScrollContainer.CheckIfObjectsOnScreen();
                }

                DraggingInLastFrame = true;
            }
            else if (DraggingInLastFrame)
            {
                if (WasPlayingBeforeDragging && AudioEngine.Track.IsPaused || (AudioEngine.Track.IsStopped && !AudioEngine.Track.HasPlayed))
                    AudioEngine.Track.Play();

                DraggingInLastFrame = false;
                WasPlayingBeforeDragging = false;
            }
        }
    }
}
