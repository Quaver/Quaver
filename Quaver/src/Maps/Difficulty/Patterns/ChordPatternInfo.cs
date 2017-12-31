using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Maps.Difficulty.Patterns
{
    internal class ChordPatternInfo : IPattern
    {
        /// <summary>
        ///     The total time the pattern takes
        /// </summary>
        public int TotalTime { get; set; }

        /// <summary>
        ///     The time the pattern starts
        /// </summary>
        public int StartingObjectTime { get; set; }

        /// <summary>
        ///     The list of objects in the pattern
        /// </summary>
        public List<HitObjectInfo> HitObjects { get; set; }

        /// <summary>
        ///     The type of chord this pattern is
        /// </summary>
        public ChordType ChordType { get; set; }
    }
}
