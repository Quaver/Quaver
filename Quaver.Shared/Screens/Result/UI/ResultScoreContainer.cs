/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Window;

namespace Quaver.Shared.Screens.Result.UI
{
    public class ResultScoreContainer : Sprite
    {
        /// <summary>
        ///     Reference to the parent screen
        /// </summary>
        public ResultScreen Screen { get; }

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
        private SpriteTextBitmap TextScoreResults { get; set; }

        /// <summary>
        ///     The divider line at the bottom of the box.
        /// </summary>
        public Sprite BottomHorizontalDividerLine { get; private set; }

        /// <summary>
        ///     The header text that displays "Statistcs"
        /// </summary>
        private SpriteTextBitmap TextStatistics { get; set; }

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

        /// <summary>
        ///     The sprite containing the cached texture of the hit difference.
        /// </summary>
        private Sprite HitDifferenceGraph { get; set; }

        /// <summary>
        ///     Raw hit difference graph. Used to draw it to a RenderTarget2D
        ///     <see cref="CacheHitDifferenceGraph"/>
        /// </summary>
        private ResultHitDifferenceGraph HitDifferenceGraphRaw { get; }

        /// <summary>
        ///     A ScoreProcessor which is more likely to be filled with hit stats than the one in
        ///     ResultScreen. For example, this one will have stats loaded from a replay.
        ///
        ///     TODO: this should really be the ResultScreen processor.
        /// </summary>
        private ScoreProcessor Processor { get; }

        /// <summary>
        ///     The standardied scoring processor
        /// </summary>
        public ScoreProcessor StandardizedProcessor { get; }

        /// <summary>
        ///     Hit statistics computed for the current score.
        /// </summary>
        private HitStatistics HitStatistics { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ResultScoreContainer(ResultScreen screen, ScoreProcessor standardizedProcessor = null)
        {
            Screen = screen;
            Size = new ScalableVector2(WindowManager.Width - 56, 490);
            Image = UserInterface.ResultScorePanel;
            DestroyIfParentIsNull = false;
            Processor = Screen.GetScoreProcessor();
            StandardizedProcessor = standardizedProcessor;

            if (Processor.Stats != null)
                HitStatistics = Processor.GetHitStatistics();
            else
                HitStatistics = new HitStatistics();

            CreateTopHorizontalDividerLine();
            CreateHeaderBackground();
            CreateBottomHorizontalDividerLine();
            CreateVerticalDividerLine();
            CreateScoreResultsText();
            CreateStatisticsText();
            CreateKeyValueItems();
            CreateStatisticsKeyValueItems();
            CreateOffsetFixButtons();
            CreateJudgementBreakdown();
            CreateOnlineStats();

            // Create the graph but don't set a constructor, as we need to draw it to a RenderTarget2D
            HitDifferenceGraphRaw = new ResultHitDifferenceGraph(new ScalableVector2(Width - VerticalDividerLine.X - 30, 200), Processor, ResultScreen.Map);
        }

        public override void Update(GameTime gameTime)
        {
            if (!Visible)
                return;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            CacheHitDifferenceGraph(gameTime);
            base.Draw(gameTime);
        }

        public override void Destroy()
        {
            HitDifferenceGraph?.Image?.Dispose();

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateHeaderBackground() => HeaderBackground = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(Width - 2 * 2, TopHorizontalDividerLine.Y - TopHorizontalDividerLine.Height),
            Tint = Color.Black,
            Alpha = 0.45f,
            Y = 2,
            X = 2,
        };

        /// <summary>
        /// </summary>
        private void CreateTopHorizontalDividerLine() => TopHorizontalDividerLine = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(Width - 4, 1),
            X = 2,
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
            Size = new ScalableVector2(Width - 4, 1),
            X = 2,
            Y = Height - 50,
            Alpha = 0.60f,
        };

        /// <summary>
        ///     Creates the text that says "Score Results"
        /// </summary>
        private void CreateScoreResultsText()
        {
            TextScoreResults = new SpriteTextBitmap(FontsBitmap.GothamRegular, "RESULTS")
            {
                Parent = this,
                Y = TopHorizontalDividerLine.Y / 2f,
                X = VerticalDividerLine.X / 2f,
                FontSize = 18
            };

            TextScoreResults.Y -= TextScoreResults.Height / 2f;
            TextScoreResults.X -= TextScoreResults.Width / 2f;
        }

        /// <summary>
        ///     Creates the text that says "Statistics"
        /// </summary>
        private void CreateStatisticsText()
        {
            TextStatistics = new SpriteTextBitmap(FontsBitmap.GothamRegular, "STATISTICS")
            {
                Parent = this,
                Y = TopHorizontalDividerLine.Y / 2f,
                X = VerticalDividerLine.X + (Width - VerticalDividerLine.X)  / 2f,
                FontSize = 18
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
                    if (Screen.ScoreProcessor.Failed)
                        performanceRating = 0;
                    else
                        performanceRating = new RatingProcessorKeys(MapManager.Selected.Value.DifficultyFromMods(Screen.ScoreProcessor.Mods)).CalculateRating(Screen.Gameplay.Ruleset.StandardizedReplayPlayer.ScoreProcessor);
                    break;
                case ResultScreenType.Replay:
                    if (Screen.ScoreProcessor.Failed)
                        performanceRating = 0;
                    else
                        performanceRating = new RatingProcessorKeys(MapManager.Selected.Value.DifficultyFromMods(Screen.ScoreProcessor.Mods)).CalculateRating(Screen.ScoreProcessor);
                    break;
                case ResultScreenType.Score:
                    performanceRating = Screen.Score.PerformanceRating;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ResultKeyValueItem standardized = null;

            int spacing;
            const int beginPosition = 40;

            if (StandardizedProcessor != null || Screen.ResultsType == ResultScreenType.Score && !Screen.Score.IsOnline)
            {
                var acc = StandardizedProcessor?.Accuracy ?? Screen.Score.RankedAccuracy;

                standardized = new ResultKeyValueItem(ResultKeyValueItemType.Vertical, "RANKED ACCURACY",
                    StringHelper.AccuracyToString((float) acc));

                spacing = 44;
            }
            else
                spacing = 100;

            ResultKeyValueItems = new List<ResultKeyValueItem>()
            {
                new ResultKeyValueItem(ResultKeyValueItemType.Vertical, "SCORE RATING", $"{performanceRating:F}"),
                new ResultKeyValueItem(ResultKeyValueItemType.Vertical, "TOTAL SCORE", $"{Screen.ScoreProcessor.Score:N0}"),
                new ResultKeyValueItem(ResultKeyValueItemType.Vertical, "ACCURACY", StringHelper.AccuracyToString(Screen.ScoreProcessor.Accuracy)),
            };

            if (standardized != null)
                ResultKeyValueItems.Add(standardized);

            ResultKeyValueItems.Add(new ResultKeyValueItem(ResultKeyValueItemType.Vertical, "MAX COMBO", $"{Screen.ScoreProcessor.MaxCombo}x"));

            for (var i = 0; i < ResultKeyValueItems.Count; i++)
            {
                var item = ResultKeyValueItems[i];
                item.Parent = this;
                item.Y = TopHorizontalDividerLine.Y + 15;

                if (i == 0)
                {
                    item.X = beginPosition;
                    continue;
                }

                item.X = ResultKeyValueItems[i - 1].X + ResultKeyValueItems[i - 1].Width + spacing;
            }

            // Add a divider line at the bottom of the key value items
            var firstItem = ResultKeyValueItems.First();
            ResultKeyValueItemDividerLine = new Sprite
            {
                Parent = this,
                Y = firstItem.Y + firstItem.TextValue.Y + firstItem.TextValue.Height + 15,
                Size = new ScalableVector2(Width, 1),
                Alpha = 0.45f
            };
        }

        /// <summary>
        ///     Creates all of the statistics items.
        /// </summary>
        private void CreateStatisticsKeyValueItems()
        {
            var meanItem = new ResultKeyValueItem(ResultKeyValueItemType.Vertical, "AVERAGE", $"{-HitStatistics.Mean:F} ms")
            {
                Parent = this,
                Y = TopHorizontalDividerLine.Y + 15
            };
            var standardDeviationItem = new ResultKeyValueItem(ResultKeyValueItemType.Vertical, "STANDARD DEVIATION", $"{HitStatistics.StandardDeviation:F} ms")
            {
                Parent = this,
                Y = TopHorizontalDividerLine.Y + 15
            };

            var availableWidth = Width - VerticalDividerLine.X;
            var padding = (availableWidth - meanItem.Width - standardDeviationItem.Width) / 3;

            meanItem.X = VerticalDividerLine.X + padding;
            standardDeviationItem.X = Width - padding - standardDeviationItem.Width;
        }

        /// <summary>
        ///     Creates the local and global offset fix buttons.
        /// </summary>
        private void CreateOffsetFixButtons()
        {
            // Don't draw the buttons if we don't have the hit stats (and therefore don't know the
            // values to adjust the offset by).
            if (Processor.Stats == null)
                return;

            var availableWidth = Width - VerticalDividerLine.X;
            var buttonPadding = 15;
            var buttonWidth = (availableWidth - buttonPadding * 3) / 2;

            var localOffsetButton = new BorderedTextButton("Fix Local Offset", Colors.MainAccent,
                (o, e) =>
                {
                    // Local offset is scaled with rate, so the adjustment depends on the rate the
                    // score was played on.
                    var change = HitStatistics.Mean * ModHelper.GetRateFromMods(Processor.Mods);
                    var newOffset = (int) Math.Round(ResultScreen.Map.LocalOffset - change);

                    DialogManager.Show(new ConfirmCancelDialog($"Local offset will be changed from {ResultScreen.Map.LocalOffset} ms to {newOffset} ms.",
                        (o_, e_) =>
                        {
                            ResultScreen.Map.LocalOffset = newOffset;
                            MapDatabaseCache.UpdateMap(ResultScreen.Map);
                            NotificationManager.Show(NotificationLevel.Success, $"Local offset was set to {ResultScreen.Map.LocalOffset} ms.");
                        }));
                })
            {
                Parent = this,
                X = VerticalDividerLine.X + buttonPadding,
                Y = BottomHorizontalDividerLine.Y - 15 - 25,
                Height = 30,
                Width = buttonWidth,
                Text =
                {
                    Font = Fonts.SourceSansProSemiBold,
                    FontSize = 13
                }
            };
            var globalOffsetButton = new BorderedTextButton("Fix Global Offset", Colors.MainAccent,
                (o, e) =>
                {
                    var newOffset = (int) Math.Round(ConfigManager.GlobalAudioOffset.Value + HitStatistics.Mean);

                    DialogManager.Show(new ConfirmCancelDialog($"Global offset will be changed from {ConfigManager.GlobalAudioOffset.Value} ms to {newOffset} ms.",
                        (o_, e_) =>
                        {
                            ConfigManager.GlobalAudioOffset.Value = newOffset;
                            NotificationManager.Show(NotificationLevel.Success, $"Global offset was set to {ConfigManager.GlobalAudioOffset.Value} ms.");
                        }));
                })
            {
                Parent = this,
                X = localOffsetButton.X + localOffsetButton.Width + buttonPadding,
                Y = BottomHorizontalDividerLine.Y - 15 - 25,
                Height = 30,
                Width = buttonWidth,
                Text =
                {
                    Font = Fonts.SourceSansProSemiBold,
                    FontSize = 13
                }
            };
        }

        /// <summary>
        /// </summary>
        private void CreateJudgementBreakdown() => JudgementBreakdown = new ResultJudgementBreakdown(this, Screen.ScoreProcessor)
        {
            Parent = this,
            Y = ResultKeyValueItemDividerLine.Y + ResultKeyValueItemDividerLine.Height,
            X = 2
        };

        /// <summary>
        /// </summary>
        private void CreateOnlineStats() => OnlineStats = new ResultOnlineStats(Screen, this)
        {
            Parent = this,
            Y = BottomHorizontalDividerLine.Y,
            X = 2
        };

        /// <summary>
        ///     Draws the hit difference graph to a RenderTarget2D
        /// </summary>
        private void CacheHitDifferenceGraph(GameTime gameTime)
        {
            if (HitDifferenceGraph != null)
                return;

            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception e)
            {
                // ignored
            }

            var (pixelWidth, pixelHeight) = HitDifferenceGraphRaw.AbsoluteSize * WindowManager.ScreenScale;

            var renderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, (int) pixelWidth, (int) pixelHeight, false,
                GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            GameBase.Game.GraphicsDevice.SetRenderTarget(renderTarget);
            HitDifferenceGraphRaw.SpriteBatchOptions = new SpriteBatchOptions {BlendState = BlendState.Opaque};
            HitDifferenceGraphRaw.Draw(gameTime);
            GameBase.Game.SpriteBatch.End();

            Texture2D outputTexture = renderTarget;

            GameBase.Game.GraphicsDevice.SetRenderTarget(null);

            HitDifferenceGraphRaw.Destroy();

            HitDifferenceGraph = new Sprite
            {
                Parent = this,
                Image = outputTexture,
                Size = HitDifferenceGraphRaw.Size,
                X = VerticalDividerLine.X + (Width - VerticalDividerLine.X) / 2f - HitDifferenceGraphRaw.Width / 2f,
                Y = BottomHorizontalDividerLine.Y - 15 - 200 - 15 - 25,
                SpriteBatchOptions = new SpriteBatchOptions {BlendState = BlendState.AlphaBlend},
            };
        }
    }
}
