/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Globalization;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Helpers;
using SQLite;

namespace Quaver.Shared.Database.Scores
{
    /// <summary>
    ///     The following is all the schema of data that will be stored in the local scores database
    ///     When retrieving data from the scores db, this is all the data that will be able to be
    ///     accessed
    /// </summary>
    public class Score
    {
        /// <summary>
        ///     The Id of the score
        /// </summary>
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        ///     The MD5 Hash of the map
        /// </summary>
        public string MapMd5 { get; set; }

        /// <summary>
        ///     The name of the player who set the score
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The date and time the score was achieved
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        ///     The score the player achieved
        /// </summary>
        public int TotalScore { get; set; }

        /// <summary>
        ///     The grade achieved for this score
        /// </summary>
        public Grade Grade { get; set; }

        /// <summary>
        ///     The accuracy the player achieved
        /// </summary>
        public double Accuracy { get; set; }

        /// <summary>
        ///     The max combo the player achieved
        /// </summary>
        public int MaxCombo { get; set; }

        /// <summary>
        ///     The amount of marvs the user got.
        /// </summary>
        public int CountMarv { get; set; }

        /// <summary>
        ///     The amount of perfs the user got.
        /// </summary>
        public int CountPerf { get; set; }

        /// <summary>
        ///     The amount of greats the user got.
        /// </summary>
        public int CountGreat { get; set; }

        /// <summary>
        ///     The amount of goods the user got.
        /// </summary>
        public int CountGood { get; set; }

        /// <summary>
        ///     The amount of okays the user got.
        /// </summary>
        public int CountOkay { get; set; }

        /// <summary>
        ///     The amount of misses the user got.
        /// </summary>
        public int CountMiss { get; set; }

        /// <summary>
        ///     Integer based seed used for shuffling the lanes when randomize mod is active.
        ///     Defaults to -1 if there is no seed.
        /// </summary>
        [Ignore]
        public int RandomizeModifierSeed { get; set; } = -1;
        
        /// <summary>
        ///     Bitwise sum of the mods used in the play
        /// </summary>
        public ModIdentifier Mods { get; set; }

        /// <summary>
        ///     The game mode for this local score.
        /// </summary>
        public GameMode Mode { get; set; }

        /// <summary>
        ///     The scroll speed the player used during this play.
        /// </summary>
        public int ScrollSpeed { get; set; }

        /// <summary>
        ///     String that contains the judgement breakdown
        /// </summary>
        public string JudgementBreakdown { get; set; }

        /// <summary>
        ///     The amount of times paused during the score.
        /// </summary>
        public int PauseCount { get; set; }

        /// <summary>
        ///     The performance rating of the score
        /// </summary>
        public double PerformanceRating { get; set; }

        /// <summary>
        ///     The version of the rating calculator that the score was played on.
        /// </summary>
        public string RatingProcessorVersion { get; set; }

        /// <summary>
        ///     If the score is an online score.
        /// </summary>
        [Ignore]
        public bool IsOnline { get; set; }

        /// <summary>
        ///     The user's steam id that submitted the score.
        /// </summary>
        [Ignore]
        public long SteamId { get; set; }

        /// <summary>
        ///     Creates a local score object from a score processor.
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="md5"></param>
        /// <param name="name"></param>
        /// <param name="scrollSpeed"></param>
        /// <param name="pauseCount"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static Score FromScoreProcessor(ScoreProcessor processor, string md5, string name, int scrollSpeed, int pauseCount, int seed)
        {
            var score = new Score()
            {
                MapMd5 = md5,
                Name = name,
                DateTime = $"{System.DateTime.Now.ToShortDateString()} {System.DateTime.Now.ToShortTimeString()}",
                Mode = processor.Map.Mode,
                TotalScore = processor.Score,
                Grade = processor.Failed ? Grade.F : GradeHelper.GetGradeFromAccuracy(processor.Accuracy),
                Accuracy = processor.Accuracy,
                MaxCombo = processor.MaxCombo,
                CountMarv = processor.CurrentJudgements[Judgement.Marv],
                CountPerf = processor.CurrentJudgements[Judgement.Perf],
                CountGreat = processor.CurrentJudgements[Judgement.Great],
                CountGood = processor.CurrentJudgements[Judgement.Good],
                CountOkay = processor.CurrentJudgements[Judgement.Okay],
                CountMiss = processor.CurrentJudgements[Judgement.Miss],
                Mods = processor.Mods,
                ScrollSpeed = scrollSpeed,
                PauseCount =  pauseCount,
                RandomizeModifierSeed = seed,
                JudgementBreakdown = GzipHelper.Compress(processor.GetJudgementBreakdown()),
            };

            return score;
        }

        /// <summary>
        ///     Converts an OnlineScoreboardScore to a local score.
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public static Score FromOnlineScoreboardScore(OnlineScoreboardScore score)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970,1,1,0,0,0,0,DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(score.Timestamp / 1000f).ToLocalTime();

            var localScore = new Score()
            {
                IsOnline = true,
                Id = score.Id,
                SteamId = score.SteamId,
                MapMd5 = score.MapMd5,
                Name = score.Username,
                DateTime = dtDateTime.ToString(CultureInfo.InvariantCulture),
                Mode = score.Mode,
                TotalScore = score.TotalScore,
                PerformanceRating = score.PerformanceRating,
                Grade = GradeHelper.GetGradeFromAccuracy((float) score.Accuracy),
                Accuracy = score.Accuracy,
                MaxCombo = score.MaxCombo,
                CountMarv = score.CountMarv,
                CountPerf = score.CountPerf,
                CountGreat = score.CountGreat,
                CountGood = score.CountGood,
                CountOkay = score.CountOkay,
                CountMiss = score.CountMiss,
                Mods = score.Mods
            };

            return localScore;
        }

        /// <summary>
        ///     Converts the score object into a blank replay.
        /// </summary>
        /// <returns></returns>
        public Replay ToReplay() => new Replay(Mode, Name, Mods, MapMd5)
        {
            Date = Convert.ToDateTime(DateTime, CultureInfo.InvariantCulture),
            Score = TotalScore,
            Accuracy = (float)Accuracy,
            MaxCombo = MaxCombo,
            CountMarv = CountMarv,
            CountPerf = CountPerf,
            CountGreat = CountGreat,
            CountGood = CountGood,
            CountOkay = CountOkay,
            CountMiss = CountMiss,
            RandomizeModifierSeed = RandomizeModifierSeed
        };
    }
}
