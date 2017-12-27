using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Maps.Difficulty.Structures
{
    internal struct VibroPatternInfo : IPattern
    {
        /// <summary>
        ///     The amount of time the pattern lasts
        /// </summary>
        public int TotalTime { get; set; }

        /// <summary>
        ///     The key lane this vibro pattern takes place in
        /// </summary>
        public int Lane { get; set; }

        /// <summary>
        ///     The list of HitObjects in this vibro pattern
        /// </summary>
        public List<HitObjectInfo> HitObjects { get; set; }
    }
}
