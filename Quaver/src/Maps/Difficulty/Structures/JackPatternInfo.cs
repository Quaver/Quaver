using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Maps.Difficulty.Structures
{
    internal class JackPatternInfo : IPattern
    {
        /// <summary>
        ///     The total time this pattern takes
        /// </summary>
        public int TotalTime { get; set; }

        /// <summary>
        ///     The time of the hit object where the pattern begins
        /// </summary>
        public int StartingObjectTime { get; set; }

        /// <summary>
        ///     The lane the pattern takes place in
        /// </summary>
        public int Lane { get; set; }

        /// <summary>
        ///     The list of HitObjects in this pattern
        /// </summary>
        public List<HitObjectInfo> HitObjects { get; set; }
    }
}
