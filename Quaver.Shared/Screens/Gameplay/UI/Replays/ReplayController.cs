using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Gameplay.UI.Replays
{
    public class ReplayController : Sprite
    {
        /// <summary>
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <summary>
        /// </summary>
        private ProgressBar SongTimeProgressBar { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton ProgressBarButton { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus CurrentTime { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus TimeLeft { get; set; }

        /// <summary>
        /// </summary>
        private PausePlayButton PausePlayButton { get; set; }

        /// <summary>
        /// </summary>
        private IconButton FastForwardButton { get; set; }

        /// <summary>
        /// </summary>
        private IconButton BackwardButton { get; set; }

        /// <summary>
        /// </summary>
        private IconButton RestartButton { get; set; }

        /// <summary>
        /// </summary>
        private IconButton SkipToEndButton { get; set; }

        /// <summary>
        /// </summary>
        private IconButton SpeedButton { get; set; }

        /// <summary>
        /// </summary>
        private IconButton ToggleSVButton { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus F1ToHide { get; set; }

        /// <summary>
        /// </summary>
        private const int BUTTON_SPACING = 22;

        /// <summary>
        /// </summary>
        private ReplayControllerSpeed SpeedSlider { get; set; }

        /// <summary>
        /// </summary>
        public ReplayController(GameplayScreen screen)
        {
            Screen = screen;

            Size = new ScalableVector2(500, 124);
            Tint = ColorHelper.HexToColor("#181818");
            AddBorder(Colors.MainAccent, 2);

            CreateProgressBar();
            CreateCurrentTime();
            CreateTimeLeft();
            CreatePausePlayButton();
            CreateFastForwardButton();
            CreateBackwardButton();
            CreateRestartButton();
            CreateSkipToEndButton();
            CreateSpeedButton();
            CreateToggleSvButton();
            CreateHidePrompt();
            CreateSpeedSlider();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (DialogManager.Dialogs.Count == 0)
            {
                if (KeyboardManager.IsUniqueKeyPress(Keys.F1))
                {
                    Visible = !Visible;
                    GameBase.Game.GlobalUserInterface.Cursor.Alpha = Visible ? 1 : 0;
                }
            }

            UpdateProgressBar();
            UpdateTimeTexts();

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void UpdateProgressBar() => SongTimeProgressBar.Bindable.Value = AudioEngine.Track?.Time ?? 0;

        /// <summary>
        /// </summary>
        private void CreateProgressBar()
        {
            SongTimeProgressBar = new ProgressBar(new Vector2(Width * 0.72f, 14), 0, Screen?.Map?.Length ?? 0,
                AudioEngine.Track?.Time ?? 0,
                Color.LightGray, Colors.MainAccent)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = 34
            };

            ProgressBarButton = new ImageButton(UserInterface.BlankBox)
            {
                Parent = SongTimeProgressBar,
                Size = SongTimeProgressBar.Size,
                Alpha = 0
            };

            ProgressBarButton.Clicked += (sender, args) =>
            {
                var percentage = (MouseManager.CurrentState.X - ProgressBarButton.AbsolutePosition.X) / ProgressBarButton.AbsoluteSize.X;
                Screen?.HandleReplaySeeking(AudioEngine.Track.Length * percentage);
            };
        }

        /// <summary>
        /// </summary>
        private void CreateCurrentTime()
        {
            CurrentTime = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "00:00", 22)
            {
                Parent = SongTimeProgressBar,
                Alignment = Alignment.MidLeft,
                X = -10
            };

            CurrentTime.X -= CurrentTime.Width;
        }

        /// <summary>
        /// </summary>
        private void CreateTimeLeft()
        {
            TimeLeft = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "-00:00", 22)
            {
                Parent = SongTimeProgressBar,
                Alignment = Alignment.MidRight,
                X = -CurrentTime.X + 4
            };
        }

        /// <summary>
        /// </summary>
        private void CreatePausePlayButton()
        {
            PausePlayButton = new PausePlayButton()
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Y = -SongTimeProgressBar.Y + 10,
                Size = new ScalableVector2(26, 26)
            };

            PausePlayButton.Clicked += (sender, args) =>
            {
                if (Screen == null)
                    return;

                Screen.IsPaused = !Screen.IsPaused;
            };
        }

        /// <summary>
        /// </summary>
        private void CreateFastForwardButton()
        {
            FastForwardButton = new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_forward_button))
            {
                Parent = this,
                Alignment = PausePlayButton.Alignment,
                Y = PausePlayButton.Y,
                Size = PausePlayButton.Size,
                X = PausePlayButton.X + PausePlayButton.Width + BUTTON_SPACING
            };

            FastForwardButton.Clicked += (sender, args) => Screen?.HandleReplaySeeking(AudioEngine.Track.Time + 10000);
        }

        /// <summary>
        /// </summary>
        private void CreateBackwardButton()
        {
            BackwardButton = new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_forward_button))
            {
                Parent = this,
                SpriteEffect = SpriteEffects.FlipHorizontally,
                Alignment = PausePlayButton.Alignment,
                Y = PausePlayButton.Y,
                Size = PausePlayButton.Size,
                X = PausePlayButton.X - PausePlayButton.Width - BUTTON_SPACING
            };

            BackwardButton.Clicked += (sender, args) => Screen?.HandleReplaySeeking(AudioEngine.Track.Time - 10000);
        }

        /// <summary>
        /// </summary>
        private void CreateRestartButton()
        {
            RestartButton = new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_step_backward))
            {
                Parent = this,
                Alignment = PausePlayButton.Alignment,
                Y = PausePlayButton.Y,
                Size = PausePlayButton.Size,
                X = BackwardButton.X - BackwardButton.Width - BUTTON_SPACING + 6
            };

            RestartButton.Clicked += (sender, args) => Screen?.HandleReplaySeeking(0);
        }

        /// <summary>
        /// </summary>
        private void CreateSkipToEndButton()
        {
            SkipToEndButton = new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_step_forward))
            {
                Parent = this,
                Alignment = PausePlayButton.Alignment,
                Y = PausePlayButton.Y,
                Size = PausePlayButton.Size,
                X = FastForwardButton.X + FastForwardButton.Width + BUTTON_SPACING - 6
            };

            SkipToEndButton.Clicked += (sender, args) => Screen?.HandleReplaySeeking(Screen.Map.Length);
        }

        /// <summary>
        /// </summary>
        private void CreateSpeedButton()
        {
            SpeedButton = new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_time))
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Y = PausePlayButton.Y,
                Size = PausePlayButton.Size,
                X = 26
            };

            SpeedButton.Clicked += (sender, args) => SpeedSlider.Visible = !SpeedSlider.Visible;
        }

        /// <summary>
        /// </summary>
        private void CreateToggleSvButton()
        {
            ToggleSVButton = new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_lightning_bolt_shadow))
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                Y = PausePlayButton.Y,
                Size = PausePlayButton.Size,
                X = -SpeedButton.X,
                Visible = false
            };
        }

        /// <summary>
        /// </summary>
        private void CreateHidePrompt()
        {
            F1ToHide = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "PRESS F1 TO SHOW/HIDE", 20)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Y = -6
            };

            F1ToHide.Y -= F1ToHide.Height;
        }

        /// <summary>
        /// </summary>
        private void CreateSpeedSlider()
        {
            SpeedSlider = new ReplayControllerSpeed(Screen, new ScalableVector2(Width * 0.50f, 40))
            {
                Parent = this,
                Y = -40,
            };

            SpeedSlider.Y -= SpeedSlider.Height;
        }

        /// <summary>
        /// </summary>
        private void UpdateTimeTexts()
        {
            var currTime = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds((int) SongTimeProgressBar.Bindable.Value);
            CurrentTime.Text = currTime.ToString("mm:ss");

            var timeLeft = new DateTime(1970, 1, 1)
                           + TimeSpan.FromMilliseconds((int)SongTimeProgressBar.Bindable.MaxValue - SongTimeProgressBar.Bindable.Value);

            // Set the new value.
            TimeLeft.Text = "-" + timeLeft.ToString("mm:ss");
        }
    }
}