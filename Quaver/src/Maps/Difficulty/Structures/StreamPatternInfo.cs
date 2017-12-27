﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Maps.Difficulty.Structures
{
    internal struct StreamPatternInfo : IPattern
    {
        /// <summary>
        ///     The amount of time the pattern lasts
        /// </summary>
        public int TotalTime { get; set; }

        /// <summary>
        ///     The time of the hit object where the pattern begins
        /// </summary>
        public int StartingObjectTime { get; set; }

        /// <summary>
        ///     The list of HitObjects in this vibro pattern
        /// </summary>
        public List<HitObjectInfo> HitObjects { get; set; }
    }
}
