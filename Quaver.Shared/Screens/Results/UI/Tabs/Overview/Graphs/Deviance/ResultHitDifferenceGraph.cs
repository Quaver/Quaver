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
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.Results;
using Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs;
using Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Deviance;
using TagLib.Matroska;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Result.UI
{
    public class ResultHitDifferenceGraph : Sprite
    {
        /// <summary>
        ///     The size of each hit dot.
        /// </summary>
        private const float DotSize = 3;

        /// <summary>
        ///     The size of each miss dot.
        /// </summary>
        private const float MissDotSize = 5;

        /// <summary>
        ///     The width of each miss line.
        /// </summary>
        private const float MissLineWidth = 2;

        /// <summary>
        ///     Color of mine hits
        /// </summary>
        private static readonly Color MineHitColor = new(179, 179, 179);

        /// <summary>
        ///     The default alpha of a judgement window area line, before any highlighting is applied.
        /// </summary>
        private const float JudgementAreaLineAlpha = 0.5f;

        /// <summary>
        ///     The alpha applied to dots/miss-lines that don't match the current highlight. Unlike the
        ///     judgement window lines, dots already sit at full alpha at rest so they have no headroom to brighten.
        /// </summary>
        private const float DimmedDotAlpha = 0.25f;

        /// <summary>
        ///     The largest of the dot sizes. Used for things like minimum graph width and dot positioning.
        /// </summary>
        private static float MaxDotSize => Math.Max(DotSize, MissDotSize);

        /// <summary>
        ///     The score processor.
        /// </summary>
        private ScoreProcessor Processor { get; set; }

        /// <summary>
        ///     The map of the stats
        /// </summary>
        private Map Map { get; set; }

        /// <summary>
        ///     Hit stats with meaningful hit differences which are drawn as regular dots. This includes hits
        ///     within the judgement range. LN release hit differences are scaled down by their multiplier.
        /// </summary>
        private List<HitStat> StatsWithHitDifference { get; set; }

        /// <summary>
        ///     Hit stats without meaningful hit differences which are drawn as centered dots. This includes hits
        ///     outside of the judgement range (early LN releases) and misses (including late LN releases).
        /// </summary>
        private List<HitStat> StatsWithoutHitDifference { get; set; }

        /// <summary>
        ///     The largest hit window.
        /// </summary>
        private float LargestHitWindow { get; }

        /// <summary>
        /// </summary>
        public List<HitDifferenceGraphLineData> LineData { get; } = new List<HitDifferenceGraphLineData>();

        /// <summary>
        /// </summary>
        public Sprite MiddleLine { get; private set; }

        /// <summary>
        ///     Every hit dot that was drawn, along with the judgement/mine metadata needed to highlight it.
        ///     Misses are represented on this graph by a dot only. the separate combo-break miss line
        ///     (<see cref="CreateMissLines"/>) is a distinct visual and is intentionally left out of the
        ///     highlight system so a single miss doesn't get highlighted twice over.
        /// </summary>
        private List<(Sprite Sprite, Judgement Judgement, bool IsMine, float BaseAlpha)> Dots { get; } =
            new List<(Sprite, Judgement, bool, float)>();

        /// <summary>
        ///     Line shown at the mean hit difference when hovering "Mean" (or the combined zone).
        /// </summary>
        private Sprite MeanLine { get; set; }

        /// <summary>
        ///     Line shown at -StandardDeviation when hovering "Std. Dev." (or the combined zone).
        /// </summary>
        private Sprite StdDevLineLow { get; set; }

        /// <summary>
        ///     Line shown at +StandardDeviation when hovering "Std. Dev." (or the combined zone).
        /// </summary>
        private Sprite StdDevLineHigh { get; set; }

        /// <summary>
        ///     The mean/standard deviation of the hit differences, used to position <see cref="MeanLine"/>
        ///     and the std. dev. lines.
        /// </summary>
        private HitStatistics Statistics { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="processor"></param>
        /// <param name="map"></param>
        /// <param name="fakeStats"></param>
        public ResultHitDifferenceGraph(ScalableVector2 size, ScoreProcessor processor, Map map,
            bool fakeStats = false)
        {
            Tint = Color.Black;
            Alpha = 0f;
            Size = size;
            Map = map;

            Processor = processor;
            LargestHitWindow = Processor.JudgementWindow.Values.Max();

            CreateJudgementAreas();
            CreateMiddleLine();

            // Make some fake hits for debugging.
            if (fakeStats)
                CreateFakeHitStats();

            // Draw the dots if there are any.
            if (Processor.Stats != null)
            {
                FilterHitStats();
                CreateMissLines();
                CreateDotsWithHitDifference();
                CreateDotsWithoutHitDifference();
            }

            Statistics = Processor.Stats != null ? Processor.GetHitStatistics() : new HitStatistics();
            CreateMeanAndStdDevLines();
        }

        /// <summary>
        ///     Converts hit difference to Y position on the graph.
        /// </summary>
        /// <param name="hitDifference"></param>
        /// <returns></returns>
        private float HitDifferenceToY(float hitDifference) => (hitDifference / LargestHitWindow) * (Height / 2);

        /// <summary>
        ///     Converts song time to X position on the graph.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private float TimeToX(float time)
        {
            // Processor.Map is null when loading in a replay
            var totalLength = Map.SongLength;
            if (totalLength == 0)
                return Width / 2;

            return time * ((Width - MaxDotSize) / totalLength) + MaxDotSize / 2;
        }

        /// <summary>
        ///     Creates fake hit stats for debugging.
        /// </summary>
        private void CreateFakeHitStats()
        {
            Processor.Stats = new List<HitStat>();

            for (var hitDifference = 0; hitDifference <= (int) Processor.JudgementWindow.Values.Max(); hitDifference++)
            {
                var judgement = Processor.JudgementWindow.Where(x => hitDifference <= x.Value).OrderBy(x => x.Value)
                    .First().Key;

                foreach (var k in new[] {-1, 1})
                {
                    Processor.Stats.Add(new HitStat(HitStatType.Hit, KeyPressType.Press, null,
                        hitDifference * 2, judgement, k * hitDifference, 0, 0));
                }
            }
        }

        /// <summary>
        ///     Creates the middle line at 0 ms hit difference.
        /// </summary>
        // ReSharper disable once ObjectCreationAsStatement
        private void CreateMiddleLine() => MiddleLine = new Sprite
        {
            Parent = this,
            Alpha = 0f,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(Width, 2),
        };

        /// <summary>
        ///     Creates the judgement area backgrounds.
        /// </summary>
        private void CreateJudgementAreas()
        {
            var sortedJudgementWindows = Processor.JudgementWindow.OrderBy(x => x.Value).ToList();

            // Compute the judgement area sizes.
            var judgementAreaSizes = new List<float>
            {
                sortedJudgementWindows[0].Value
            };

            for (var i = 1; i < sortedJudgementWindows.Count; i++)
                judgementAreaSizes.Add(sortedJudgementWindows[i].Value - sortedJudgementWindows[i - 1].Value);

            // Create the judgement area backgrounds.
            foreach (var ((judgement, difference), windowSize)
                in sortedJudgementWindows.Zip(judgementAreaSizes, (a, b) => (a, b)))
            {
                var height = HitDifferenceToY(windowSize);

                // There are two areas: one above the middle and one below the middle.
                foreach (var k in new[] {-1, 1})
                {
                    // ReSharper disable once ObjectCreationAsStatement
                    var line = new Sprite()
                    {
                        Parent = this,
                        Alpha = JudgementAreaLineAlpha,
                        Tint = ResultsJudgementGraphJudgementBar.GetColor(judgement),
                        Alignment = Alignment.MidCenter,
                        Y = k * HitDifferenceToY(difference) - k * height,
                        Size = new ScalableVector2(Width, 2),
                    };

                    LineData.Add(new HitDifferenceGraphLineData(judgement, line, (difference - windowSize) * k));
                }
            }
        }

        /// <summary>
        ///     Creates the (initially hidden) overlay lines shown when hovering Mean/Std. Dev.
        /// </summary>
        private void CreateMeanAndStdDevLines()
        {
            // ReSharper disable once ObjectCreationAsStatement
            MeanLine = new Sprite
            {
                Parent = this,
                Alpha = 0f,
                Tint = Color.White,
                Alignment = Alignment.MidCenter,
                Y = HitDifferenceToY((float) Statistics.Mean),
                Size = new ScalableVector2(Width, 2),
            };

            // ReSharper disable once ObjectCreationAsStatement
            StdDevLineLow = new Sprite
            {
                Parent = this,
                Alpha = 0f,
                Tint = Color.White,
                Alignment = Alignment.MidCenter,
                Y = -HitDifferenceToY((float) Statistics.StandardDeviation),
                Size = new ScalableVector2(Width, 2),
            };

            // ReSharper disable once ObjectCreationAsStatement
            StdDevLineHigh = new Sprite
            {
                Parent = this,
                Alpha = 0f,
                Tint = Color.White,
                Alignment = Alignment.MidCenter,
                Y = HitDifferenceToY((float) Statistics.StandardDeviation),
                Size = new ScalableVector2(Width, 2),
            };
        }

        /// <summary>
        ///     Applies a highlight state, dimming/hiding the lines and dots that don't match it.
        /// </summary>
        /// <param name="state"></param>
        public void ApplyHighlight(DevianceHighlight state)
        {
            switch (state.Type)
            {
                case DevianceHighlightType.Judgement:
                    foreach (var line in LineData)
                        line.Line.Alpha = line.Judgement == state.Judgement ? 1f : JudgementAreaLineAlpha;

                    foreach (var dot in Dots)
                        dot.Sprite.Alpha = dot.Judgement == state.Judgement && !dot.IsMine ? 1f : DimmedDotAlpha;

                    MeanLine.Alpha = 0f;
                    StdDevLineLow.Alpha = 0f;
                    StdDevLineHigh.Alpha = 0f;
                    break;
                case DevianceHighlightType.MineMiss:
                    foreach (var line in LineData)
                        line.Line.Alpha = JudgementAreaLineAlpha;

                    foreach (var dot in Dots)
                        dot.Sprite.Alpha = dot.IsMine ? 1f : DimmedDotAlpha;

                    MeanLine.Alpha = 0f;
                    StdDevLineLow.Alpha = 0f;
                    StdDevLineHigh.Alpha = 0f;
                    break;
                case DevianceHighlightType.Mean:
                case DevianceHighlightType.StdDev:
                case DevianceHighlightType.MeanAndStdDev:
                    foreach (var line in LineData)
                        line.Line.Alpha = 0f;

                    foreach (var dot in Dots)
                        dot.Sprite.Alpha = dot.BaseAlpha;

                    MeanLine.Alpha = state.Type is DevianceHighlightType.Mean or DevianceHighlightType.MeanAndStdDev ? 1f : 0f;
                    var showStdDev = state.Type is DevianceHighlightType.StdDev or DevianceHighlightType.MeanAndStdDev;
                    StdDevLineLow.Alpha = showStdDev ? 1f : 0f;
                    StdDevLineHigh.Alpha = showStdDev ? 1f : 0f;
                    break;
                default:
                    foreach (var line in LineData)
                        line.Line.Alpha = JudgementAreaLineAlpha;

                    foreach (var dot in Dots)
                        dot.Sprite.Alpha = dot.BaseAlpha;

                    MeanLine.Alpha = 0f;
                    StdDevLineLow.Alpha = 0f;
                    StdDevLineHigh.Alpha = 0f;
                    break;
            }
        }

        /// <summary>
        ///     Creates the early and late text labels.
        /// </summary>
        private void CreateEarlyLateText()
        {
            var unscaledLargestHitWindow = LargestHitWindow / ModHelper.GetRateFromMods(Processor.Mods);

            var x = 8;
            var y = 6;

            // ReSharper disable once ObjectCreationAsStatement
            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold),
                ResultsLocalization.Get("Late hit window", unscaledLargestHitWindow), 16, false)
            {
                Parent = this,
                X = x,
                Y = y
            };

            // ReSharper disable once ObjectCreationAsStatement
            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold),
                ResultsLocalization.Get("Early hit window", unscaledLargestHitWindow), 18, false)
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                X = x,
                Y = -y,
            };
        }

        /// <summary>
        ///     Filters out only those stats that we care about and computes earliest and latest hit time.
        /// </summary>
        private void FilterHitStats()
        {
            StatsWithHitDifference = new List<HitStat>();
            StatsWithoutHitDifference = new List<HitStat>();

            foreach (var breakdown in Processor.Stats)
            {
                var hitDifference = breakdown.HitDifference;
                if (breakdown.KeyPressType == KeyPressType.Release && breakdown.Judgement != Judgement.Miss)
                {
                    // Scale LN release hit errors to match regular hits.
                    hitDifference = (int) (breakdown.HitDifference /
                                           Processor.WindowReleaseMultiplier[breakdown.Judgement]);
                }

                var hitStat = new HitStat(breakdown.Type, breakdown.KeyPressType, breakdown.HitObject,
                    breakdown.SongPosition, breakdown.Judgement, hitDifference, breakdown.Accuracy,
                    breakdown.Health);

                // No need to check for Type == Miss as all of them have hitDifference == int.MinValue.
                if (hitDifference != int.MinValue && Math.Abs(hitDifference) <= LargestHitWindow)
                    StatsWithHitDifference.Add(hitStat);
                else
                    StatsWithoutHitDifference.Add(hitStat);
            }
        }

        /// <summary>
        ///     Creates lines for misses to indicate combo breaks
        /// </summary>
        private void CreateMissLines()
        {
            foreach (var miss in Processor.Stats.FindAll(s => s.Judgement == Judgement.Miss))
            {
                var isMine = miss.HitObject.Type == HitObjectType.Mine;

                _ = new Sprite
                {
                    Parent = this,
                    Alpha = 0.35f,
                    Tint = isMine ? MineHitColor : ResultsJudgementGraphJudgementBar.GetColor(miss.Judgement),
                    Alignment = Alignment.MidLeft,
                    X = TimeToX(miss.SongPosition),
                    Y = 0,
                    Size = new ScalableVector2(MissLineWidth, Height)
                };
            }
        }

        /// <summary>
        ///     Creates dots for hits with meaningful hit differences.
        /// </summary>
        private void CreateDotsWithHitDifference()
        {
            // Exit if we don't have any dots to draw.
            if (StatsWithHitDifference.Count == 0)
                return;

            // Return if the graph isn't wide enough.
            if (Width < MaxDotSize)
                return;

            // Create a sprite for every dot.
            foreach (var breakdown in StatsWithHitDifference)
            {
                var isMine = breakdown.HitObject.Type == HitObjectType.Mine;

                var dot = new Sprite
                {
                    Parent = this,
                    Tint = ResultsJudgementGraphJudgementBar.GetColor(breakdown.Judgement),
                    Size = new ScalableVector2(DotSize, DotSize),
                    Image = FontAwesome.Get(FontAwesomeIcon.fa_circle),
                    X = (int) TimeToX(breakdown.SongPosition) - (int) (DotSize / 2),
                    Y = (int) HitDifferenceToY(breakdown.HitDifference),
                    Alignment = Alignment.MidLeft,
                };

                Dots.Add((dot, breakdown.Judgement, isMine, dot.Alpha));
            }
        }

        /// <summary>
        ///     Creates dots for hits without meaningful hit differences.
        /// </summary>
        private void CreateDotsWithoutHitDifference()
        {
            // Return if the graph isn't wide enough.
            if (Width < MaxDotSize)
                return;

            foreach (var breakdown in StatsWithoutHitDifference)
            {
                var isMine = breakdown.HitObject.Type == HitObjectType.Mine && breakdown.Type == HitStatType.Miss;

                var dot = new Sprite
                {
                    Parent = this,
                    Tint = ResultsJudgementGraphJudgementBar.GetColor(breakdown.Judgement),
                    Size = new ScalableVector2(MissDotSize, MissDotSize),
                    Image = FontAwesome.Get(FontAwesomeIcon.fa_circle),
                    X = (int) TimeToX(breakdown.SongPosition) - (int) (MissDotSize / 2),
                    Y = 0,
                    Alignment = Alignment.MidLeft,
                };

                Dots.Add((dot, breakdown.Judgement, isMine, dot.Alpha));
            }
        }
    }
}
