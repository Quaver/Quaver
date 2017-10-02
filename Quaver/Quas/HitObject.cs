using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Quas
{
    internal struct HitObject
    {
        /// <summary>
        /// The offset of in the song where the hitobject is supposed to be hit
        /// </summary>
        internal int StartTime { get; set; }

        /// <summary>
        /// The lane in which the key is in.
        /// </summary>
        internal int KeyLane { get; set; }

        /// <summary>
        /// The end time of the key (if > 0, it's an LN)
        /// </summary>
        internal int EndTime { get; set; }
    }
}
