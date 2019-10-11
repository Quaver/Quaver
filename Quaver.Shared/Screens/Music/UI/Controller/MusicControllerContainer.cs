using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Main.UI.Jukebox;
using TagLib.Ape;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.Music.UI.Controller
{
    public class MusicControllerContainer : Sprite
    {
        /// <summary>
        /// </summary>
        private MusicControllerBackground Background { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Title { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Artist { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxProgressBar ProgressBar { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxPlayPauseButton PlayPauseButton { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxBackwardsButton BackwardsButton { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxForwardsButton ForwardsButton { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus CurrentTime { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus TimeLeft { get; set; }

        /// <summary>
        /// </summary>
        public MusicControllerContainer()
        {
            Alpha = 0;
            Size = new ScalableVector2(WindowManager.Width - 622, 200);

            CreateBackground();
            CreateTitle();
            CreateArtist();
            CreateProgressBar();
            CreatePlayPauseButton();
            CreateBackwardsButton();
            CreateForwardsButton();
            CreateCurrentTime();
            CreateTimeLeft();

            SetText();
            MapManager.Selected.ValueChanged += OnMapChanged;
        }

        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;
            base.Destroy();
        }

        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => SetText();

        private void SetText()
        {
            Title.Text = MapManager.Selected.Value.Title;
            Title.TruncateWithEllipsis((int) Width - 50);

            Artist.Text = MapManager.Selected.Value.Artist;
            Artist.TruncateWithEllipsis((int) Width - 50);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (AudioEngine.Track != null)
            {
                var currTime = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(AudioEngine.Track.Position);
                CurrentTime.Text = currTime.ToString("mm:ss");

                var timeLeft = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(AudioEngine.Track.Length - AudioEngine.Track.Position);
                TimeLeft.Text = "-" + timeLeft.ToString("mm:ss");
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateBackground() => Background = new MusicControllerBackground(Size) { Parent = this };

        /// <summary>
        /// </summary>
        private void CreateTitle()
        {
            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Example Song Title", 34)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = 28,
                Tint = Colors.MainAccent
            };
        }

        /// <summary>
        /// </summary>
        private void CreateArtist()
        {
            Artist = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Music Artist", 28)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = Title.Y + Title.Height + 16
            };
        }

        /// <summary>
        /// </summary>
        private void CreateProgressBar()
        {
            ProgressBar = new JukeboxProgressBar(new Vector2(Width, 5), 0, 1000, 560,
                Color.LightGray, Colors.SecondaryAccent)
            {
                Parent = this,
                Alignment = Alignment.BotLeft
            };
        }

        /// <summary>
        /// </summary>
        private void CreatePlayPauseButton()
        {
            PlayPauseButton = new JukeboxPlayPauseButton(null)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = Artist.Y + Artist.Height + 20,
                Size = new ScalableVector2(42, 42)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateBackwardsButton()
        {
            BackwardsButton = new JukeboxBackwardsButton(null)
            {
                Parent = PlayPauseButton,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(24, 22),
                X = -50
            };
        }

        /// <summary>
        /// </summary>
        private void CreateForwardsButton()
        {
            ForwardsButton = new JukeboxForwardsButton(null)
            {
                Parent = PlayPauseButton,
                Alignment = Alignment.MidCenter,
                Size = BackwardsButton.Size,
                X = -BackwardsButton.X
            };
        }

        /// <summary>
        /// </summary>
        private void CreateCurrentTime()
        {
            CurrentTime = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "00:00", 24)
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Y = -ProgressBar.Height - 6,
                X = 10
            };
        }

        /// <summary>
        /// </summary>
        private void CreateTimeLeft()
        {
            TimeLeft = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "-00:00", CurrentTime.FontSize)
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                Y = CurrentTime.Y,
                X = -CurrentTime.X
            };
        }
    }
}