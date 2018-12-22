/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Processors.Difficulty;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Result.UI
{
    public class ResultScoreContainer : Sprite
    {
        /// <summary>
        ///     Reference to the parent screen
        /// </summary>
        private ResultScreen Screen { get; }

        /// <summary>
        ///     The divider line at the top of the box
        /// </summary>
        public Sprite TopHorizontalDividerLine { get; private set; }

        /// <summary>
        ///     The divider line that splits the container vertically
        /// </summary>
        public Sprite VerticalDividerLine { get; private set; }

        /// <summary>
        ///     The header text that displays "Score Results"
        /// </summary>
        private SpriteText TextScoreResults { get; set; }

        /// <summary>
        ///     The divider line at the bottom of the box.
        /// </summary>
        public Sprite BottomHorizontalDividerLine { get; private set; }

        /// <summary>
        ///     The header text that displays "Statistcs"
        /// </summary>
        private SpriteText TextStatistics { get; set; }

        /// <summary>
        ///     Table header background
        /// </summary>
        public Sprite HeaderBackground { get; private set; }

        /// <summary>
        ///     Displays important statistics for the score.
        /// </summary>
        private List<ResultKeyValueItem> ResultKeyValueItems { get; set; }

        /// <summary>
        ///     The horizontal divider line under the result key/value items
        /// </summary>
        public Sprite ResultKeyValueItemDividerLine { get; private set; }

        /// <summary>
        ///     Displays a graph of all the received judgements
        /// </summary>
        private ResultJudgementBreakdown JudgementBreakdown { get; set; }

        /// <summary>
        ///     Displays online stats if necessary
        /// </summary>
        private ResultOnlineStats OnlineStats { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ResultScoreContainer(ResultScreen screen)
        {
            Screen = screen;
            Size = new ScalableVector2(WindowManager.Width - 56, 450);
            Tint = Color.Black;
            Alpha = 0.45f;
            AddBorder(Color.White, 2);

            CreateTopHorizontalDividerLine();
            CreateHeaderBackground();
            CreateBottomHorizontalDividerLine();
            CreateVerticalDividerLine();
            CreateScoreResultsText();
            CreateStatisticsText();
            CreateKeyValueItems();
            CreateJudgementBreakdown();
            CreateOnlineStats();
        }

        /// <summary>
        /// </summary>
        private void CreateHeaderBackground() => HeaderBackground = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(Width - Border.Thickness * 2, TopHorizontalDividerLine.Y - TopHorizontalDividerLine.Height),
            Tint = Color.Black,
            Alpha = 0.45f,
            Y = Border.Thickness,
            X = Border.Thickness
        };

        /// <summary>
        /// </summary>
        private void CreateTopHorizontalDividerLine() => TopHorizontalDividerLine = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(Width, 1),
            Y = 50,
            Alpha = 1
        };

        /// <summary>
        /// </summary>
        private void CreateVerticalDividerLine() => VerticalDividerLine = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(1, Height - TopHorizontalDividerLine.Y - (Height - BottomHorizontalDividerLine.Y)),
            X = Width - 520,
            Alpha = 0.60f,
            Y = TopHorizontalDividerLine.Y
        };

        /// <summary>
        /// </summary>
        private void CreateBottomHorizontalDividerLine() => BottomHorizontalDividerLine = new Sprite()
        {
            Parent = this,
            Size = new ScalableVector2(Width, 1),
            Y = Height - 50,
            Alpha = 0.60f,
        };

        /// <summary>
        ///     Creates the text that says "Score Results"
        /// </summary>
        private void CreateScoreResultsText()
        {
            TextScoreResults = new SpriteText(Fonts.Exo2Medium, "RESULTS", 16)
            {
                Parent = this,
                Y = TopHorizontalDividerLine.Y / 2f,
                X = VerticalDividerLine.X / 2f,
            };

            TextScoreResults.Y -= TextScoreResults.Height / 2f;
            TextScoreResults.X -= TextScoreResults.Width / 2f;
        }

        /// <summary>
        ///     Creates the text that says "Statistics"
        /// </summary>
        private void CreateStatisticsText()
        {
            TextStatistics = new SpriteText(Fonts.Exo2Medium, "STATISTICS", 16)
            {
                Parent = this,
                Y = TopHorizontalDividerLine.Y / 2f,
                X = VerticalDividerLine.X + (Width - VerticalDividerLine.X)  / 2f,
            };

            TextStatistics.Y -= TextStatistics.Height / 2f;
            TextStatistics.X -= TextStatistics.Width / 2f;
        }

        /// <summary>
        ///     Creates all of the main ite
        /// </summary>
        private void CreateKeyValueItems()
        {
            double performanceRating;

            switch (Screen.ResultsType)
            {
                case ResultScreenType.Gameplay:
                case ResultScreenType.Replay:
                    if (Screen.ScoreProcessor.Failed)
                        performanceRating = 0;
                    else
                        performanceRating = DifficultyProcessorKeys.CalculatePlayRating(MapManager.Selected.Value.DifficultyFromMods(Screen.ScoreProcessor.Mods), Screen.ScoreProcessor.Accuracy);
                    break;
                case ResultScreenType.Score:
                    performanceRating = Screen.Score.PerformanceRating;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ResultKeyValueItems = new List<ResultKeyValueItem>()
            {
                new ResultKeyValueItem(ResultKeyValueItemType.Vertical, "SCORE RATING", $"{performanceRating:F}"),
                new ResultKeyValueItem(ResultKeyValueItemType.Vertical, "TOTAL SCORE", $"{Screen.ScoreProcessor.Score:N0}"),
                new ResultKeyValueItem(ResultKeyValueItemType.Vertical, "ACCURACY", StringHelper.AccuracyToString(Screen.ScoreProcessor.Accuracy)),
                new ResultKeyValueItem(ResultKeyValueItemType.Vertical, "MAX COMBO", $"{Screen.ScoreProcessor.MaxCombo}x"),
            };

            for (var i = 0; i < ResultKeyValueItems.Count; i++)
            {
                var item = ResultKeyValueItems[i];
                item.Parent = this;
                item.Y = TopHorizontalDividerLine.Y + 15;

                item.X = VerticalDividerLine.X / ResultKeyValueItems.Count * i + 40;
            }

            // Add a divider line at the bottom of the key value items
            var firstItem = ResultKeyValueItems.First();
            ResultKeyValueItemDividerLine = new Sprite
            {
                Parent = this,
                Y = firstItem.Y + firstItem.TextValue.Y + firstItem.TextValue.Height + 15,
                Size = new ScalableVector2(VerticalDividerLine.X, 1),
                Alpha = 0.45f
            };
        }

        /// <summary>
        /// </summary>
        private void CreateJudgementBreakdown() => JudgementBreakdown = new ResultJudgementBreakdown(this, Screen.ScoreProcessor)
        {
            Parent = this,
            Y = ResultKeyValueItemDividerLine.Y + ResultKeyValueItemDividerLine.Height,
            X = Border.Thickness
        };

        /// <summary>
        /// </summary>
        private void CreateOnlineStats() => OnlineStats = new ResultOnlineStats(Screen, this)
        {
            Parent = this,
            Y = BottomHorizontalDividerLine.Y,
            X = Border.Thickness
        };
    }
}
