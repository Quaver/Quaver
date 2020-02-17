using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Screens.Edit.UI.Footer.Time;
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        public EditorFooter(IAudioTrack track) : base(MenuBorderType.Footer, new List<Drawable>()
        {
            new IconTextButtonExit()
        }, new List<Drawable>()
        {
            new IconTextButtonTestPlay()
        })
        {
            Track = track;

            AnimatedLine.Visible = false;
            ForegroundLine.Visible = false;

            CreateSeekBar();
            CreateTimeTexts();
            CreatePausePlayButton();
        }

        /// <summary>
        /// </summary>
        private void CreateSeekBar()
        {
            SeekBar = new EditorFooterSeekBar(new BindableInt(0, 0, (int) Track.Length), new Vector2(Width, 6), Track)
            {
                Parent = this,
            };

            SeekBar.Y -= SeekBar.Height;
        }

        /// <summary>
        /// </summary>
        private void CreateTimeTexts()
        {
            const int posX = 18;

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

            CurrentTime.Y -= CurrentTime.Height + 26;
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
                Size = new ScalableVector2(40, 40)
            };
        }
    }
}