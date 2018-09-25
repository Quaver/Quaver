using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Helpers;
using SQLite;

namespace Quaver.Database.Scores
{
    /// <summary>
    ///     The following is all the schema of data that will be stored in the local scores database
    ///     When retrieving data from the scores db, this is all the data that will be able to be
    ///     accessed
    /// </summary>
    public class LocalScore
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
        public int Score { get; set; }

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
        ///     String that contains the hit breakdown the user received.
        /// </summary>
        public string HitBreakdown { get; set; }

        /// <summary>
        ///     The amount of times paused during the score.
        /// </summary>
        public int PauseCount { get; set; }

        /// <summary>
        ///     Creates a local score object from a score processor.
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="md5"></param>
        /// <param name="name"></param>
        /// <param name="scrollSpeed"></param>
        /// <returns></returns>
        public static LocalScore FromScoreProcessor(ScoreProcessor processor, string md5, string name, int scrollSpeed, int pauseCount)
        {
            var score = new LocalScore()
            {
                MapMd5 = md5,
                Name = name,
                DateTime = $"{System.DateTime.Now.ToShortDateString()} {System.DateTime.Now.ToShortTimeString()}",
                Mode = processor.Map.Mode,
                Score = processor.Score,
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
                HitBreakdown = GzipHelper.Compress(processor.GetHitBreakdown())
            };

            return score;
        }
    }
}