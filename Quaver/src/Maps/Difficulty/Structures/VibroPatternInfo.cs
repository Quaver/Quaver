using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Maps.Difficulty.Structures
{
    internal struct VibroPatternInfo
    {
        /// <summary>
        ///     The amount of time the pattern lasts
        /// </summary>
        internal int TotalTime { get; set; }

        /// <summary>
        ///     The key lane this vibro pattern takes place in
        /// </summary>
        internal int Lane { get; set; }

        /// <summary>
        ///     The list of HitObjects in this vibro pattern
        /// </summary>
        internal List<HitObjectInfo> HitObjects { get; set; }
    }
}
