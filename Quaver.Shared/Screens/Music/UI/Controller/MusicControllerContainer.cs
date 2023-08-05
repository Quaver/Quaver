using System;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Listening;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Main.UI.Jukebox;
using Quaver.Shared.Screens.Music.Components;
using TagLib.Ape;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.Music.UI.Controller
{
    public class MusicControllerContainer : ImageButton
    {
        /// <summary>
        /// </summary>
        private MusicPlayerJukebox Jukebox { get; set; }

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
        public MusicControllerContainer(MusicPlayerJukebox jukebox) : base(UserInterface.BlankBox)
        {
            Jukebox = jukebox;

            Alpha = 0;
            Size = new ScalableVector2(WindowManager.Width, 220);
            Depth = 1;

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
            Clicked += OnClicked;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            UpdateSongTime();
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBackground() => Background = new MusicControllerBackground(Size) { Parent = this };

        /// <summary>
        /// </summary>
        private void CreateTitle()
        {
            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "No Track Playing", 34)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = 34,
                Tint = ColorHelper.HexToColor("#57D6FF")
            };
        }

        /// <summary>
        /// </summary>
        private void CreateArtist()
        {
            Artist = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Unknown Artist", 28)
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
                Color.LightGray, ColorHelper.HexToColor("#57D6FF"))
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
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
            BackwardsButton = new JukeboxBackwardsButton(Jukebox)
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
            ForwardsButton = new JukeboxForwardsButton(Jukebox)
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

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => SetText();

        /// <summary>
        /// </summary>
        private void SetText()
        {
            ScheduleUpdate(() =>
            {
                var map = MapManager.Selected.Value;

                if (map == null && OnlineManager.ListeningParty != null)
                {
                    Title.Text = OnlineManager.ListeningParty.SongTitle;
                    Title.Tint = Color.Crimson;

                    Artist.Text = OnlineManager.ListeningParty.SongArtist;

                }
                else if (map != null)
                {
                    Title.Text = map.Title;
                    Title.Tint = ColorHelper.HexToColor("#57D6FF");

                    Artist.Text = map.Artist;
                }

                Title.TruncateWithEllipsis((int) Width - 50);
                Artist.TruncateWithEllipsis((int) Width - 50);
            });
        }

        /// <summary>
        /// </summary>
        private void UpdateSongTime()
        {
            if (AudioEngine.Track == null || AudioEngine.Track.IsDisposed)
                return;

            var unix = new DateTime(1970, 1, 1);

            var currTime = unix + TimeSpan.FromMilliseconds(AudioEngine.Track.Position);
            CurrentTime.Text = currTime.ToString("mm:ss");

            var timeLeft = unix + TimeSpan.FromMilliseconds(AudioEngine.Track.Length - AudioEngine.Track.Position);
            TimeLeft.Text = "-" + timeLeft.ToString("mm:ss");
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClicked(object sender, EventArgs e)
        {
            if (AudioEngine.Track == null || AudioEngine.Track.IsDisposed)
                return;

            if (!OnlineManager.IsListeningPartyHost)
                return;

            try
            {
                var percentage = (MouseManager.CurrentState.X - AbsolutePosition.X) / AbsoluteSize.X;

                lock (AudioEngine.Track)
                {
                    AudioEngine.Track.Seek(percentage * AudioEngine.Track.Length);
                    OnlineManager.UpdateListeningPartyState(ListeningPartyAction.Seek);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, LogType.Runtime);
            }
        }
    }
}