using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Quas
{
    internal struct SliderVelocity
    {
        /// <summary>
        /// The offset in the song where the SV begins
        /// </summary>
        internal float StartTime { get; set; }

        /// <summary>
        /// The multiplier of the SV - how fast or slow it is.
        /// </summary>
        internal float Multiplier { get; set; }

        /// <summary>
        /// The hitsound volume during this section.
        /// </summary>
        internal int Volume { get; set; }
    }
}
