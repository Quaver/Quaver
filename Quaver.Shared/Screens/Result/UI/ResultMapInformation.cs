/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Result.UI
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
        private SpriteTextBitmap DifficultyName { get; set; }

        /// <summary>
        ///     Displays the title of the song.
        /// </summary>
        private SpriteTextBitmap SongTitle { get; set; }

        /// <summary>
        ///     Displays the creator of the map.
        /// </summary>
        private SpriteTextBitmap MapCreator { get; set; }

        /// <summary>
        ///     Displays the player of the map.
        /// </summary>
        private SpriteTextBitmap PlayedBy { get; set; }

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
            Size = new ScalableVector2(WindowManager.Width - 56, 126);
            Image = UserInterface.ResultHeaderPanel;

            CreateThumbnail();
            CreateDifficultyName();
            CreateSongTitle();
            CreateMapCreator();
            CreatePlayerName();
            CreateGrade();

            BackgroundHelper.Loaded += OnBackgroundLoaded;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            BackgroundHelper.Loaded -= OnBackgroundLoaded;
            base.Destroy();
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
                Size = new ScalableVector2(222, 110),
                Alpha = 0
            };

            Thumbnail.AddBorder(ColorHelper.HexToColor("#236f9c"));
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

            var windows = "";

            switch (Screen.ResultsType)
            {
                case ResultScreenType.Gameplay:
                    if (JudgementWindowsDatabaseCache.Selected.Value.Name != JudgementWindowsDatabaseCache.Standard.Name)
                        windows = $" ({JudgementWindowsDatabaseCache.Selected.Value.Name})";
                    break;
                case ResultScreenType.Score:
                    if (Screen.Score.JudgementWindowPreset != null && Screen.Score.JudgementWindowPreset != JudgementWindowsDatabaseCache.Standard.Name)
                        windows = $" ({Screen.Score.JudgementWindowPreset})";
                    break;
                case ResultScreenType.Replay:
                    break;
                default:
                    break;
            }

            if (Screen.ScoreProcessor.Mods != 0)
                text += $" + {ModHelper.GetModsString(Screen.ScoreProcessor.Mods)}";

            text += windows;

            DifficultyName = new SpriteTextBitmap(FontsBitmap.GothamRegular, text)
            {
                Parent = this,
                X = Thumbnail.X + Thumbnail.Width + 14,
                Y = 14,
                FontSize = 17
            };
        }

        /// <summary>
        ///     Creates the text that displays the title of the song.
        /// </summary>
        private void CreateSongTitle()
        {
            var title = $"{Map.Artist} - {Map.Title}";

            SongTitle = new SpriteTextBitmap(FontsBitmap.GothamRegular, title)
            {
                Parent = this,
                X = DifficultyName.X,
                Y = DifficultyName.Y + DifficultyName.Height + 10,
                FontSize = 17
            };
        }

        /// <summary>
        ///     Creates the text that displays the map creator's name
        /// </summary>
        private void CreateMapCreator()
        {
            var text = $"By: {Map.Creator}";

            MapCreator = new SpriteTextBitmap(FontsBitmap.GothamRegular, text)
            {
                Parent = this,
                X = SongTitle.X,
                Y = SongTitle.Y + SongTitle.Height + 10,
                FontSize = 17
            };
        }

        /// <summary>
        ///     Creates the text that displays the name of the player
        /// </summary>
        private void CreatePlayerName()
        {
            var text = OnlineManager.CurrentGame != null ? "Match Played on" : $"Played By: {Screen.Replay.PlayerName} on";

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

            PlayedBy = new SpriteTextBitmap(FontsBitmap.GothamRegular, text)
            {
                Parent = this,
                X = SongTitle.X,
                Y = MapCreator.Y + MapCreator.Height + 10,
                FontSize = 17
            };

            PlayedBy.Visible = true;
        }

        /// <summary>
        ///     Creates the grade of the score
        /// </summary>
        private void CreateGrade()
        {
            Texture2D image;

            switch (Screen.ResultsType)
            {
                case ResultScreenType.Replay:
                case ResultScreenType.Gameplay:
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (Screen.ScoreProcessor.Failed)
                        image = SkinManager.Skin.Grades[API.Enums.Grade.F];
                    else
                        image = SkinManager.Skin.Grades[GradeHelper.GetGradeFromAccuracy(Screen.ScoreProcessor.Accuracy)];
                    break;
                case ResultScreenType.Score:
                    image = SkinManager.Skin.Grades[Screen.Score.Grade];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Grade = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -50,
                Size = new ScalableVector2(66, 66),
                Image = image
            };
        }

        /// <summary>
        ///     Called when the background of the map has been loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundLoaded(object sender, BackgroundLoadedEventArgs e) => UpdateThumbnailImage();
    }
}
