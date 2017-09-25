using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Qua
{
    public struct HitObject
    {
        /// <summary>
        /// The offset of in the song where the hitobject is supposed to be hit
        /// </summary>
        public int StartTime;

        /// <summary>
        /// The lane in which the key is in.
        /// </summary>
        public int KeyLane;

        /// <summary>
        /// The end time of the key (if > 0, it's an LN)
        /// </summary>
        public int EndTime;
    }
}
