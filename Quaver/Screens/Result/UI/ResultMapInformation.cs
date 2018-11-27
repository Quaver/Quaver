using System;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics.Backgrounds;
using Quaver.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Screens.Result.UI
{
    public class ResultMapInformation : Sprite
    {
        /// <summary>
        ///     Reference to the parent screen
        /// </summary>
        private ResultScreen Screen { get; }

        /// <summary>
        ///     Displays the background of the map
        /// </summary>
        private Sprite Thumbnail { get; set; }

        /// <summary>
        ///     Displays the difficulty name of the map
        /// </summary>
        private SpriteText DifficultyName { get; set; }

        /// <summary>
        ///     Displays the title of the song.
        /// </summary>
        private SpriteText SongTitle { get; set; }

        /// <summary>
        ///     Displays the creator of the map.
        /// </summary>
        private SpriteText MapCreator { get; set; }

        /// <summary>
        ///     Displays the player of the map.
        /// </summary>
        private SpriteText PlayedBy { get; set; }

        /// <summary>
        ///     Displays the grade achieved on the score
        /// </summary>
        private Sprite Grade { get; set; }

        /// <summary>
        ///     Quick reference to the selected map.
        /// </summary>
        private static Map Map => MapManager.Selected.Value;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ResultMapInformation(ResultScreen screen)
        {
            Screen = screen;
            Size = new ScalableVector2(WindowManager.Width - 56, 160);
            Tint = Color.Black;
            Alpha = 0.45f;

            AddBorder(Color.White, 2);

            CreateThumbnail();
            CreateDifficultyName();
            CreateSongTitle();
            CreateMapCreator();
            CreatePlayerName();
            CreateGrade();

            BackgroundHelper.Loaded += OnBackgroundLoaded;
        }

        /// <summary>
        ///     Creates the thumbnail of the map
        /// </summary>
        private void CreateThumbnail()
        {
            Thumbnail = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 10,
                Size = new ScalableVector2(256, 144),
                Alpha = 0
            };

            Thumbnail.AddBorder(Color.White);
            UpdateThumbnailImage();
        }

        /// <summary>
        ///     Updates the thumbnail whenever we've received a new one.
        /// </summary>
        private void UpdateThumbnailImage()
        {
            if (BackgroundHelper.Map == null || MapManager.GetBackgroundPath(BackgroundHelper.Map) != MapManager.GetBackgroundPath(MapManager.Selected.Value))
                return;

            Thumbnail.Image = BackgroundHelper.RawTexture;
            Thumbnail.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Thumbnail.Alpha, 1, 200));
        }

        /// <summary>
        ///     Creates the text that displays the difficulty name of the map.
        /// </summary>
        private void CreateDifficultyName()
        {
            var text = $"[{Map.DifficultyName}]";

            if (Screen.ScoreProcessor.Mods != 0)
                text += $" + {ModHelper.GetModsString(Screen.ScoreProcessor.Mods)}";

            DifficultyName = new SpriteText(BitmapFonts.Exo2SemiBold, text, 13)
            {
                Parent = this,
                X = Thumbnail.X + Thumbnail.Width + 10,
                Y = 25
            };
        }

        /// <summary>
        ///     Creates the text that displays the title of the song.
        /// </summary>
        private void CreateSongTitle()
        {
            var title = $"{Map.Artist} - {Map.Title}";

            SongTitle = new SpriteText(BitmapFonts.Exo2SemiBold, title, 13)
            {
                Parent = this,
                X = DifficultyName.X,
                Y = DifficultyName.Y + DifficultyName.Height + 8
            };
        }

        /// <summary>
        ///     Creates the text that displays the map creator's name
        /// </summary>
        private void CreateMapCreator()
        {
            var text = $"By: {Map.Creator}";

            MapCreator = new SpriteText(BitmapFonts.Exo2SemiBold, text, 13)
            {
                Parent = this,
                X = SongTitle.X,
                Y = SongTitle.Y + SongTitle.Height + 8
            };
        }

        /// <summary>
        ///     Creates the text that displays the name of the player
        /// </summary>
        private void CreatePlayerName()
        {
            var text = $"Played By: {Screen.Replay.PlayerName} on";

            switch (Screen.ResultsType)
            {
                case ResultScreenType.Gameplay:
                    var now = DateTime.Now;
                    var time = string.Format("{0:hh:mm:ss tt}", now);
                    text += $" {now.ToShortDateString()} @ {time}";
                    break;
                case ResultScreenType.Score:
                case ResultScreenType.Replay:
                    var replayTime = string.Format("{0:hh:mm:ss tt}", Screen.Replay.Date);
                    text += $" {Screen.Replay.Date.ToShortDateString()} @ {replayTime}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            PlayedBy = new SpriteText(BitmapFonts.Exo2SemiBold, text, 13)
            {
                Parent = this,
                X = SongTitle.X,
                Y = MapCreator.Y + MapCreator.Height + 8
            };
        }

        /// <summary>
        ///     Creates the grade of the score
        /// </summary>
        private void CreateGrade() => Grade = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.MidRight,
            X = -50,
            Size = new ScalableVector2(100, 100),
            Image = Screen.ScoreProcessor.Failed ? SkinManager.Skin.Grades[API.Enums.Grade.F] :
                SkinManager.Skin.Grades[GradeHelper.GetGradeFromAccuracy(Screen.ScoreProcessor.Accuracy)]
        };

        /// <summary>
        ///     Called when the background of the map has been loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundLoaded(object sender, BackgroundLoadedEventArgs e) => UpdateThumbnailImage();
    }
}