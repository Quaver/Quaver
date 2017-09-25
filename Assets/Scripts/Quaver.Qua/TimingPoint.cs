
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Qua
{
    public struct TimingPoint
    {
        /// <summary>
        /// The offset in the song where the Timing Point begins.
        /// </summary>
        public float StartTime;

        /// <summary>
        /// The BPM of the particular timing point
        /// </summary>
        public float BPM;
    }
}
