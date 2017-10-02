using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Quas
{
    internal struct TimingPoint
    {
        /// <summary>
        /// The offset in the song where the Timing Point begins.
        /// </summary>
        internal float StartTime { get; set; }

        /// <summary>
        /// The BPM of the particular timing point
        /// </summary>
        internal float Bpm { get; set; }
    }
}
