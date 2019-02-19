/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.API.Replays;
using Quaver.API.Replays.Virtual;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Logging;

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
        ///     The largest of the dot sizes. Used for things like minimum graph width and dot positioning.
        /// </summary>
        private static float MaxDotSize => Math.Max(DotSize, MissDotSize);

        /// <summary>
        ///     The score processor.
        /// </summary>
        private ScoreProcessor Processor { get; set; }

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
        ///     Time of the first hit. Set in FilterHitStats().
        /// </summary>
        private int EarliestHitTime { get; set; }

        /// <summary>
        ///     Time of the last hit. Set in FilterHitStats().
        /// </summary>
        private int LatestHitTime { get; set; }

        /// <summary>
        ///     The largest hit window.
        /// </summary>
        private float LargestHitWindow { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="screen"></param>
        public ResultHitDifferenceGraph(ScalableVector2 size, ResultScreen screen)
        {
            Tint = Color.Black;
            Alpha = 0.2f;
            Size = size;

            Processor = GetScoreProcessor(screen);
            LargestHitWindow = Processor.JudgementWindow.Values.Max();

            CreateMiddleLine();
            CreateJudgementAreas();

            // Make some fake hits for debugging.
            // CreateFakeHitStats();

            // Draw the dots if there are any.
            if (Processor.Stats != null)
            {
                FilterHitStats();
                CreateDotsWithHitDifference();
                CreateDotsWithoutHitDifference();
            }

            CreateEarlyLateText();

            AddBorder(Color.White);
            Border.Alpha = 0.6f;
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
            var totalLength = LatestHitTime - EarliestHitTime;

            if (totalLength == 0)
                return Width / 2;

            return (time - EarliestHitTime) * ((Width - MaxDotSize) / totalLength) + MaxDotSize / 2;
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
        ///     Returns the score processor to use. Loads hit stats from a replay if needed.
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static ScoreProcessor GetScoreProcessor(ResultScreen screen)
        {
            // If we already have stats (for example, this is a result screen right after a player finished playing a map), use them.
            if (screen.ScoreProcessor.Stats != null)
                return screen.ScoreProcessor;

            // Otherwise, get the stats from a replay.
            Replay replay = null;

            // FIXME: unify this logic with watching a replay from a ResultScreen.
            try
            {
                switch (screen.ResultsType)
                {
                    case ResultScreenType.Gameplay:
                    case ResultScreenType.Replay:
                        replay = screen.Replay;
                        break;
                    case ResultScreenType.Score:
                        // Don't do anything for online replays since they aren't downloaded yet.
                        if (!screen.Score.IsOnline)
                            replay = new Replay($"{ConfigManager.DataDirectory.Value}/r/{screen.Score.Id}.qr");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, "Unable to read replay file");
                Logger.Error(e, LogType.Runtime);
            }

            // Load a replay if we got one.
            if (replay == null)
                return screen.ScoreProcessor;

            var qua = ResultScreen.Map.LoadQua();
            qua.ApplyMods(replay.Mods);

            var player = new VirtualReplayPlayer(replay, qua);
            player.PlayAllFrames();

            return player.ScoreProcessor;
        }

        /// <summary>
        ///     Creates the middle line at 0 ms hit difference.
        /// </summary>
        // ReSharper disable once ObjectCreationAsStatement
        private void CreateMiddleLine() => new Sprite
        {
            Parent = this,
            Alpha = 0.7f,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(Width, 1),
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
                    new Sprite()
                    {
                        Parent = this,
                        Alpha = 0.3f,
                        Tint = SkinManager.Skin.Keys[GameMode.Keys4].JudgeColors[judgement],
                        Alignment = Alignment.MidCenter,
                        Y = k * HitDifferenceToY(difference) - k * height / 2,
                        Size = new ScalableVector2(Width, height),
                    };
                }
            }
        }

        /// <summary>
        ///     Creates the early and late text labels.
        /// </summary>
        private void CreateEarlyLateText()
        {
            var unscaledLargestHitWindow = LargestHitWindow / ModHelper.GetRateFromMods(Processor.Mods);

            // ReSharper disable once ObjectCreationAsStatement
            new SpriteText(Fonts.SourceSansProSemiBold, $"Late (+{unscaledLargestHitWindow}ms)", 13)
            {
                Parent = this,
                X = 2
            };

            // ReSharper disable once ObjectCreationAsStatement
            new SpriteText(Fonts.SourceSansProSemiBold, $"Early (-{unscaledLargestHitWindow}ms)", 13)
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                X = 2
            };
        }

        /// <summary>
        ///     Filters out only those stats that we care about and computes earliest and latest hit time.
        /// </summary>
        private void FilterHitStats()
        {
            StatsWithHitDifference = new List<HitStat>();
            StatsWithoutHitDifference = new List<HitStat>();
            EarliestHitTime = int.MaxValue;
            LatestHitTime = int.MinValue;

            foreach (var breakdown in Processor.Stats)
            {
                EarliestHitTime = Math.Min(EarliestHitTime, breakdown.SongPosition);
                LatestHitTime = Math.Max(LatestHitTime, breakdown.SongPosition);

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
                // ReSharper disable once ObjectCreationAsStatement
                new Sprite
                {
                    Parent = this,
                    Tint = SkinManager.Skin.Keys[GameMode.Keys4].JudgeColors[breakdown.Judgement],
                    Size = new ScalableVector2(DotSize, DotSize),
                    Image = FontAwesome.Get(FontAwesomeIcon.fa_circle),
                    X = (int) TimeToX(breakdown.SongPosition) - (int) (DotSize / 2),
                    Y = (int) HitDifferenceToY(breakdown.HitDifference),
                    Alignment = Alignment.MidLeft,
                };
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
                // ReSharper disable once ObjectCreationAsStatement
                new Sprite
                {
                    Parent = this,
                    Tint = SkinManager.Skin.Keys[GameMode.Keys4].JudgeColors[breakdown.Judgement],
                    Size = new ScalableVector2(MissDotSize, MissDotSize),
                    Image = FontAwesome.Get(FontAwesomeIcon.fa_circle),
                    X = (int) TimeToX(breakdown.SongPosition) - (int) (MissDotSize / 2),
                    Y = 0,
                    Alignment = Alignment.MidLeft,
                };
            }
        }
    }
}
