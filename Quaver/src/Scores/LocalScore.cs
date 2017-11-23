using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Quaver.Scores
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
        ///     The MD5 Hash of the beatmap 
        /// </summary>
        public string BeatmapMd5 { get; set; }

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
        ///     The accuracy the player achieved
        /// </summary>
        public double Accuracy { get; set; }

        /// <summary>
        ///     The max combo the player achieved
        /// </summary>
        public int MaxCombo { get; set; }

        /// <summary>
        ///     The number of marv presses
        /// </summary>
        public int MarvPressCount { get; set; }

        /// <summary>
        ///     The number of marv releases
        /// </summary>
        public int MarvReleaseCount { get; set; }

        /// <summary>
        ///     The number of Perf presses
        /// </summary>
        public int PerfPressCount { get; set; }

        /// <summary>
        ///     The number of perf releases
        /// </summary>
        public int PerfReleaseCount { get; set; }

        /// <summary>
        ///     The number of great presses
        /// </summary>
        public int GreatPressCount { get; set; }

        /// <summary>
        ///     The number of great releases
        /// </summary>
        public int GreatReleaseCount { get; set; }

        /// <summary>
        ///     The number of good presses
        /// </summary>
        public int GoodPressCount { get; set; }

        /// <summary>
        ///     The number of good releases
        /// </summary>
        public int GoodReleaseCount { get; set; }

        /// <summary>
        ///     The number of okay presses
        /// </summary>
        public int OkayPressCount { get; set; }

        /// <summary>
        ///     The number of okay releases
        /// </summary>
        public int OkayReleaseCount { get; set; }

        /// <summary>
        ///     The number of misses for the score.
        /// </summary>
        public int Misses { get; set; }

        /// <summary>
        ///     The "rating" for the achieved play.
        /// </summary>
        public float Rating { get; set; }

        /// <summary>
        ///     Bitwise sum of the mods used in the play
        /// </summary>
        public int Mods { get; set; }

        /// <summary>
        ///     The scroll speed the player used during this play.
        /// </summary>
        public int ScrollSpeed { get; set; }

        /// <summary>
        ///     The replay data for this particular score.
        /// </summary>
        public string ReplayData { get; set; }
    }
}
