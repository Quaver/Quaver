using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps.Structures;

namespace Quaver.Screens.Gameplay.Rulesets.HitObjects
{
    public abstract class GameplayHitObject
    {
        /// <summary>
        ///     The info of this particular HitObject from the map file.
        /// </summary>
        public HitObjectInfo Info { get; set; }

        /// <summary>
        ///     The list of possible beat snaps.
        /// </summary>
        private static int[] BeatSnaps { get; } = { 48, 24, 16, 12, 8, 6, 4, 3 };

        /// <summary>
        ///     The beat snap index
        ///     (See: BeatSnaps array)
        /// </summary>
        public int SnapIndex { get; set; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="info"></param>
        protected GameplayHitObject(HitObjectInfo info) => Info = info;

        /// <summary>
        ///     Gets the timing point this object is in range of.
        /// </summary>
        /// <returns></returns>
        public TimingPointInfo GetTimingPoint(List<TimingPointInfo> timingPoints)
        {
            // If the object's time is greater than the time of the last timing point, return the last point
            if (Info.StartTime >= timingPoints.Last().StartTime)
                return timingPoints.Last();

            // Search through the entire list for the correct point
            for (var i = 0; i < timingPoints.Count - 1; i++)
            {
                if (Info.StartTime < timingPoints.Last().StartTime)
                {
                    return timingPoints[i];
                }
            }

            // Otherwise just return first point if we can't find it. 
            // Qua file won't be considered valid if it doesn't have at least one timing point.
            return timingPoints.First();
        }

        /// <summary>
        ///     Returns color of note beatsnap
        /// </summary>
        /// <param name="hitObject"></param>
        /// <param name="timingPoint"></param>
        /// <returns></returns>
        public static int GetBeatSnap(HitObjectInfo info, TimingPointInfo timingPoint)
        {
            // Add 2ms offset buffer space to offset and get beat length
            var pos = info.StartTime - timingPoint.StartTime + 2;
            var beatlength = 60000 / timingPoint.Bpm;

            // subtract pos until it's less than beat length. multiple loops for efficiency
            while (pos >= beatlength * ( 1 << 16 )) pos -= beatlength * ( 1 << 16 );
            while (pos >= beatlength * ( 1 << 12 )) pos -= beatlength * ( 1 << 12 );
            while (pos >= beatlength * ( 1 << 8 )) pos -= beatlength * ( 1 << 8 );
            while (pos >= beatlength * ( 1 << 4 )) pos -= beatlength * ( 1 << 4 );
            while (pos >= beatlength) pos -= beatlength;

            // Calculate Note's snap index
            var index = (int) ( Math.Floor(48 * pos / beatlength) );

            // Return Color of snap index
            for (var i = 0; i < 8; i++)
            {
                if (index % BeatSnaps[i] == 0)
                {
                    return i;
                }
            }

            // If it's not snapped to 1/16 or less, return 1/48 snap color
            return 8;
        }

        /// <summary>
        ///     Destroys the HitObject
        /// </summary>
        public abstract void Destroy();
    }
}