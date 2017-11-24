using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Modifiers;

namespace Quaver.Replays
{
    internal class Replay
    {
        /// <summary>
        ///     The version of Quaver the replay was created with (MD5 hash of the exe)
        /// </summary>
        internal string QuaverVersion { get; set; }

        /// <summary>
        ///     The MD5 of the beatmap played 
        /// </summary>
        internal string BeatmapMd5 { get; set; }

        /// <summary>
        ///     The MD5 hash of the actual replay
        /// </summary>
        internal string ReplayMd5 { get; set; }

        /// <summary>
        ///     The name of the player that made the replay
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        ///     The date and time the replay was made
        /// </summary>
        internal DateTime Date { get; set; }

        /// <summary>
        ///     The bitwise combination of mods that were used during the replay
        /// </summary>
        internal List<IMod> Mods { get; set; }

        /// <summary>
        ///     The scroll speed the player used during this play.
        /// </summary>
        public int ScrollSpeed { get; set; }

        /// <summary>
        ///     The score of the replay
        /// </summary>
        internal int Score { get; set; }

        /// <summary>
        ///     The accuracy achieved on the replay
        /// </summary>
        internal float Accuracy { get; set; }
        
        /// <summary>
        ///     The max combo achieved on the replay
        /// </summary>
        internal int MaxCombo { get; set; }

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
        ///     The list of replay frame data contained in the replay
        /// </summary>
        public List<ReplayFrame> ReplayFrames { get; set; }
    }
}
