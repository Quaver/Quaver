using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps;

namespace Quaver.States.Gameplay.HitObjects
{
    internal abstract class HitObject
    {
        /// <summary>
        ///     The info of this particular HitObject from the map file.
        /// </summary>
        internal HitObjectInfo Info { get; set; }

        /// <summary>
        ///     Initializes the HitObject's sprite.
        /// </summary>
        /// <param name="playfield"></param>
        internal abstract void InitializeSprite(IGameplayPlayfield playfield);

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="info"></param>
        internal HitObject(HitObjectInfo info)
        {
            Info = info;
        }

        /// <summary>
        ///     Gets the timing point this object is in range of.
        /// </summary>
        /// <returns></returns>
        internal TimingPointInfo GetTimingPoint(List<TimingPointInfo> timingPoints)
        {
            // If the start time of the object is greater than the last timing point, then return the last 
            // point.
            if (Info.StartTime >= timingPoints.Last().StartTime)
                return timingPoints.Last();

            // Otherwise loop through all the timing points to find the correct one.
            return timingPoints.Where((t, i) => Info.StartTime < timingPoints[i + 1].StartTime).FirstOrDefault();
        }

        /// <summary>
        ///     Returns color of note beatsnap
        /// </summary>
        /// <param name="timingPoint"></param>
        /// <returns></returns>
        internal BeatSnap GetBeatSnap(TimingPointInfo timingPoint)
        {
            // Add 2ms offset buffer space to offset and get beat length
            var pos = Info.StartTime - timingPoint.StartTime + 2;
            var beatlength = 60000 / timingPoint.Bpm;

            // subtract pos until it's less than beat length. multiple loops for efficiency
            while (pos >= beatlength * (1 << 16)) pos -= beatlength * (1 << 16);           
            while (pos >= beatlength * (1 << 12)) pos -= beatlength * (1 << 12);
            while (pos >= beatlength * (1 << 8))  pos -= beatlength * (1 << 8);
            while (pos >= beatlength * (1 << 4))  pos -= beatlength * (1 << 4);            
            while (pos >= beatlength)  pos -= beatlength;

            // If it's not snapped to 1/16 or less, return 1/48 snap color
            var snap = Math.Floor(48 * pos / beatlength);    
            return (BeatSnap) snap;
        }
    }
}