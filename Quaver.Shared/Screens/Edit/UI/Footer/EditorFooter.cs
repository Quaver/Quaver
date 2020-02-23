using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Menu.Border.Components.Buttons;
using Quaver.Shared.Screens.Edit.UI.Footer.Time;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using TagLib.Matroska;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Footer
{
    public class EditorFooter : MenuBorder
    {
        /// <summary>
        /// </summary>
        private IAudioTrack Track { get; }

        /// <summary>
        /// </summary>
        private EditorFooterSeekBar SeekBar { get; set; }

        /// <summary>
        /// </summary>
        private EditorFooterTime CurrentTime { get; set; }

        /// <summary>
        /// </summary>
        private EditorFooterTime TimeLeft { get; set; }

        /// <summary>
        /// </summary>
        private PausePlayButton PausePlayButton { get; set; }

        /// <summary>
        /// </summary>
        public IconButton FastForwardButton { get; set; }

        /// <summary>
        /// </summary>
        public IconButton BackwardButton { get; set; }

        /// <summary>
        /// </summary>
        public IconButton RestartButton { get; set; }

        /// <summary>
        /// </summary>
        public IconButton SkipToEndButton { get; set; }

        /// <summary>
        /// </summary>
        private const int BUTTON_SPACING = 24;

        /// <summary>
        /// </summary>
        private const int BUTTON_SIZE = 20;

        /// <summary>
        /// </summary>
        public static int HEIGHT { get; } = 50;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        public EditorFooter(EditScreen screen, IAudioTrack track) : base(MenuBorderType.Footer, new List<Drawable>()
        {
            new IconTextButtonExit(),
            new IconTextButtonOptions()
        }, new List<Drawable>()
        {
            new IconTextButtonTestPlay(),
            new IconTextButtonBeatSnap(screen)
        })
        {
            Track = track;

            AnimatedLine.Visible = false;
            ForegroundLine.Visible = false;

            CreateSeekBar();
            CreateTimeTexts();
            CreatePausePlayButton();
            CreateFastForwardButton();
            CreateBackwardButton();
            CreateRestartButton();
            CreateSkipToEndButton();

            Height = HEIGHT;
        }

        /// <summary>
        /// </summary>
        private void CreateSeekBar()
        {
            SeekBar = new EditorFooterSeekBar(new BindableInt(0, 0, (int) Track.Length), new Vector2(Width, 4), Track)
            {
                Parent = this,
            };

            SeekBar.Y -= SeekBar.Height;
        }

        /// <summary>
        /// </summary>
        private void CreateTimeTexts()
        {
            const int posX = 14;

            CurrentTime = new EditorFooterTime(EditorFooterTimeType.Current, FontManager.GetWobbleFont(Fonts.LatoBlack), Track)
            {
                Parent = this,
                X = posX,
            };

            TimeLeft = new EditorFooterTime(EditorFooterTimeType.Left, FontManager.GetWobbleFont(Fonts.LatoBlack), Track)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                X = -posX,
            };

            CurrentTime.Y -= CurrentTime.Height + 22;
            TimeLeft.Y = CurrentTime.Y;
        }

        /// <summary>
        /// </summary>
        private void CreatePausePlayButton()
        {
            PausePlayButton = new PausePlayButton(UserInterface.JukeboxPauseButton, UserInterface.JukeboxPlayButton, Track)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(34, 34)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateFastForwardButton()
        {
            FastForwardButton = new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_forward_button))
            {
                Parent = PausePlayButton,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(BUTTON_SIZE, BUTTON_SIZE),
                X = BUTTON_SIZE + BUTTON_SPACING
            };

            FastForwardButton.Clicked += (sender, args) =>
            {
                Track.Seek(MathHelper.Clamp((float) (Track.Time + 10000), 0, (float) Track.Length - 100));
            };
        }

        /// <summary>
        /// </summary>
        private void CreateBackwardButton()
        {
            BackwardButton = new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_forward_button))
            {
                Parent = PausePlayButton,
                SpriteEffect = SpriteEffects.FlipHorizontally,
                Alignment = Alignment.MidLeft,
                Size = FastForwardButton.Size,
                X = -BUTTON_SIZE - BUTTON_SPACING
            };

            BackwardButton.Clicked += (sender, args) =>
            {
                Track.Seek(MathHelper.Clamp((float) (Track.Time - 10000), 0, (float) Track.Length - 100));
            };
        }

        /// <summary>
        /// </summary>
        private void CreateRestartButton()
        {
            RestartButton = new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_step_backward))
            {
                Parent = PausePlayButton,
                Alignment = Alignment.MidLeft,
                Size = FastForwardButton.Size,
                X = BackwardButton.X - BackwardButton.Width - BUTTON_SPACING
            };

            RestartButton.Clicked += (sender, args) => Track.Seek(0);
        }

        /// <summary>
        /// </summary>
        private void CreateSkipToEndButton()
        {
            SkipToEndButton = new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_step_forward))
            {
                Parent = PausePlayButton,
                Alignment = Alignment.MidRight,
                Size = FastForwardButton.Size,
                X = FastForwardButton.X + FastForwardButton.Width + BUTTON_SPACING
            };

            SkipToEndButton.Clicked += (sender, args) =>
            {
                Track.Seek(MathHelper.Clamp((float) Track.Length - 100, 0, (float) Track.Length - 100));
            };
        }
    }
}