using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Audio;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Notifications;
using Quaver.Helpers;
using Quaver.Screens.Gameplay.UI;
using Quaver.Skinning;
using Wobble.Audio.Tracks;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;

namespace Quaver.Screens.Menu.UI.Jukebox
{
    public class Jukebox : Sprite
    {
        /// <summary>
        ///     The background that contains the text that says "Jukebox"
        /// </summary>
        public Sprite TitleBackground { get; set; }

        /// <summary>
        ///     The text that says "Jukebox"
        /// </summary>
        public SpriteTextBitmap JukeboxText { get; set; }

        /// <summary>
        ///     Button to select the previous track.
        /// </summary>
        public ImageButton PreviousButton { get; set; }

        /// <summary>
        ///     The button to play the track.
        /// </summary>
        public ImageButton PlayButton { get; set; }

        /// <summary>
        ///     The button to pause/unpause the track.
        /// </summary>
        public ImageButton PauseButton { get; set; }

        /// <summary>
        ///     The button to select the next track.
        /// </summary>
        public ImageButton NextButton { get; set; }

        /// <summary>
        ///     The container that holds the song title.
        /// </summary>
        public ScrollContainer SongTitleContainer { get; set; }

        /// <summary>
        ///     The text that displays the song title.
        /// </summary>
        public SpriteTextBitmap SongTitleText { get; set; }

        /// <summary>
        ///     The song time progress bar.
        /// </summary>
        public ProgressBar ProgressBar { get; set; }

        /// <summary>
        ///     Selects new random maps to play.
        /// </summary>
        public Random RNG { get; set; } = new Random();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public Jukebox()
        {
            Size = new ScalableVector2(614, 40);
            Tint = Color.Black;
            Alpha = 0.55f;

            TitleBackground = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(124, Height),
                Tint = Color.Black,
                Alpha = 0.45f
            };

            AddBorder(Color.White, 2);

            JukeboxText = new SpriteTextBitmap(BitmapFonts.Exo2SemiBold, "Jukebox", 24, Color.White,
                Alignment.MidLeft, int.MaxValue)
            {
                Parent = TitleBackground,
                Alignment = Alignment.MidCenter,
                SpriteBatchOptions = new SpriteBatchOptions()
                {
                    BlendState = BlendState.NonPremultiplied
                }
            };

            JukeboxText.Size = new ScalableVector2(JukeboxText.Width * 0.60f, JukeboxText.Height * 0.60f);

            CreateSongTitleContainer();
            CreateProgressBar();

            // Add the contained drawable afterwards, so that it appears on top of the progress bar.
            SongTitleContainer.AddContainedDrawable(SongTitleText);

            CreateControlButtons();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Animate the song title text.
            if (SongTitleText.Transformations.Count == 0)
            {
                SongTitleText.X = SongTitleText.Width + 200;

                SongTitleText.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear,
                    SongTitleText.X, -SongTitleText.Width, 6000));
            }

            // Set progress bar time.
            if (AudioEngine.Track != null)
            {
                ProgressBar.Bindable.MaxValue = AudioEngine.Track.Length;
                ProgressBar.Bindable.MinValue = 0;
                ProgressBar.Bindable.Value = AudioEngine.Track.Position;
            }

            // Start selecting random tracks.
            if (MapManager.Mapsets.Count != 0 && AudioEngine.Track == null || AudioEngine.Track.HasPlayed && AudioEngine.Track.IsStopped)
                SelectRandomTrack();

            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the container that displays the song title.
        /// </summary>
        private void CreateSongTitleContainer()
        {
            SongTitleContainer = new ScrollContainer(new ScalableVector2(Width - TitleBackground.Width - 100 - Height * 0.50f - 10, Height),
                new ScalableVector2(Width - TitleBackground.Width - 100 - Height * 0.50f - 10, Height))
            {
                Parent = this,
                X = TitleBackground.Width,
                Alpha = 0
            };

            SongTitleText = new SpriteTextBitmap(BitmapFonts.Exo2Medium, " ",
                24, Color.White, Alignment.MidCenter, int.MaxValue)
            {
                Y = 2,
                Alignment = Alignment.MidLeft
            };

            SongTitleText.Text = MapManager.Selected.Value != null ?
                $"{MapManager.Selected.Value.Artist} - {MapManager.Selected.Value.Title}"
                : $"No tracks available to play";

            SongTitleText.Size = new ScalableVector2(SongTitleText.Width * 0.50f, SongTitleText.Height * 0.50f);
            SongTitleText.X = SongTitleText.Width + 200;
            SongTitleContainer.AddContainedDrawable(SongTitleText);
        }

        /// <summary>
        ///     Creates the jukebox buttons
        /// </summary>
        private void CreateControlButtons()
        {
            NextButton = new ImageButton(FontAwesome.StepForward)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Height * 0.50f, Height * 0.50f),
                X = -10
            };

            NextButton.Clicked += (o, e) =>
            {
                SkinManager.Skin.SoundClick.CreateChannel().Play();

                SelectRandomTrack();
                NotificationManager.Show(NotificationLevel.Info, "Selecting next track");
            };

            PauseButton = new ImageButton(FontAwesome.Pause)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Height * 0.50f, Height * 0.50f),
                X = NextButton.X - NextButton.Width - 10
            };

            PauseButton.Clicked += (o, e) =>
            {
                SkinManager.Skin.SoundClick.CreateChannel().Play();

                if (AudioEngine.Track == null || AudioEngine.Track.IsDisposed)
                    return;

                string action;

                if (AudioEngine.Track.IsStopped || AudioEngine.Track.IsPaused)
                {
                    AudioEngine.Track.Play();
                    action = "Resumed Track";
                }
                else
                {
                    AudioEngine.Track.Pause();
                    action = "Paused Track";
                }

                NotificationManager.Show(NotificationLevel.Info, action);
            };

            PlayButton = new ImageButton(FontAwesome.Play)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Height * 0.50f, Height * 0.50f),
                X = PauseButton.X - PauseButton.Width - 10
            };

            PlayButton.Clicked += (o, e) =>
            {
                SkinManager.Skin.SoundClick.CreateChannel().Play();

                try
                {
                    AudioEngine.LoadCurrentTrack();
                    AudioEngine.Track?.Play();
                }
                catch (Exception)
                {
                    Logger.Error($"Failed to load track", LogType.Runtime);
                }

                NotificationManager.Show(NotificationLevel.Info, "Play");
            };

            PreviousButton = new ImageButton(FontAwesome.StepBackward)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Height * 0.50f, Height * 0.50f),
                X = PlayButton.X - PlayButton.Width - 10
            };

            PreviousButton.Clicked += (o, e) =>
            {
                SkinManager.Skin.SoundClick.CreateChannel().Play();
                NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet");
            };
        }

        /// <summary>
        ///     Selects a random map to be selected. (and for the track to play.)
        /// </summary>
        private void SelectRandomTrack()
        {
            if (MapManager.Mapsets.Count == 0)
                return;

            var randomSet = RNG.Next(0, MapManager.Mapsets.Count);
            var randomMap = RNG.Next(0, MapManager.Mapsets[randomSet].Maps.Count);

            MapManager.Selected.Value = MapManager.Mapsets[randomSet].Maps[randomMap];

            try
            {
                AudioEngine.LoadCurrentTrack();
                AudioEngine.Track.Play();
            }
            catch (Exception e)
            {
                Logger.Error($"Track for map: could not be loaded.", LogType.Runtime);
            }

            SongTitleText.Text = MapManager.Selected.Value != null ?
                $"{MapManager.Selected.Value.Artist} - {MapManager.Selected.Value.Title}"
                : $"No tracks available to play";

            SongTitleText.Size = new ScalableVector2(SongTitleText.Width * 0.50f, SongTitleText.Height * 0.50f);
            SongTitleText.X = SongTitleText.Width + 200;

            // Clear transformations
            SongTitleText.Transformations.Clear();

            // Update
            Logger.Debug($"Selected random jukebox track: {MapManager.Selected.Value.Artist} - {MapManager.Selected.Value.Title} " +
                         $"[{MapManager.Selected.Value.DifficultyName}] ", LogType.Runtime);
        }

        /// <summary>
        ///     Creates the jukebox progress bar.
        /// </summary>
        private void CreateProgressBar()
        {
            ProgressBar = new ProgressBar(new Vector2(SongTitleContainer.Width, SongTitleContainer.Height - 4), 0,
                AudioEngine.Track != null ? AudioEngine.Track.Length : int.MaxValue, 0, Color.Transparent, Colors.MainAccent)
            {
                Alignment = Alignment.MidLeft,
                ActiveBar =
                {
                    Alpha = 0.1f
                }
            };

            SongTitleContainer.AddContainedDrawable(ProgressBar);
        }
    }
}