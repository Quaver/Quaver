using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Qua
{
    public struct SliderVelocity
    {
        /// <summary>
        /// The offset in the song where the SV begins
        /// </summary>
        public float StartTime;

        /// <summary>
        /// The multiplier of the SV - how fast or slow it is.
        /// </summary>
        public float Multiplier;

        /// <summary>
        /// The hitsound volume during this section.
        /// </summary>
        public int Volume;
    }
}
