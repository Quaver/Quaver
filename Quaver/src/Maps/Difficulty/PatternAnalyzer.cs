using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Maps.Difficulty.Structures;

namespace Quaver.Maps.Difficulty
{
    internal static class PatternAnalyzer
    {
        /// <summary>
        ///     The minimum BPM to be considered vibro
        /// </summary>
        private static int VibroBaseBpm { get; set; } = 160;

        /// <summary>
        ///     The minimum BPM to be considered jacks
        /// </summary>
        private static int JackBaseBpm { get; set; } = 95;

        /// <summary>
        ///     Detects vibro patterns for ther entire map
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <param name="KeyCount"></param>
        /// <returns></returns>
        internal static List<JackPatternInfo> DetectAllLanePatterns(IReadOnlyList<HitObjectInfo> hitObjects, bool detectVibro)
        {
            return DetectLanePatterns(hitObjects.Where(x => x.Lane == 1). ToList(), detectVibro)
                .Concat(DetectLanePatterns(hitObjects.Where(x => x.Lane == 2). ToList(), detectVibro))
                .Concat(DetectLanePatterns(hitObjects.Where(x => x.Lane == 3). ToList(), detectVibro))
                .Concat(DetectLanePatterns(hitObjects.Where(x => x.Lane == 4). ToList(), detectVibro))
                .Concat(DetectLanePatterns(hitObjects.Where(x => x.Lane == 5). ToList(), detectVibro))
                .Concat(DetectLanePatterns(hitObjects.Where(x => x.Lane == 6). ToList(), detectVibro))
                .Concat(DetectLanePatterns(hitObjects.Where(x => x.Lane == 7). ToList(), detectVibro))
                .ToList();
        }

        /// <summary>
        ///     Detects vibro patterns per key lane
        ///     
        ///     A vibro pattern can be considered as 4 notes in succession for a lane
        ///     with the objects having a 1/4th millisecond difference of a base bpm
        /// <param name="hitObjects"></param>
        /// <returns></returns>
        private static List<JackPatternInfo> DetectLanePatterns(IReadOnlyList<HitObjectInfo> hitObjects, bool detectVibro)
        { 
            var baseBpm = (detectVibro) ? VibroBaseBpm : JackBaseBpm;
            var requiredPatternObjects = (detectVibro) ? 4 : 2; // The amount of objects required to be considered a pattern (Vibro : Jack)
            var detectedPatterns = new List<JackPatternInfo>(); // Detected patterns
            var patternObjects = new List<HitObjectInfo>(); // The current pattern's objects

            // Begin analyzing patterns
            for (var i = 1; i < hitObjects.Count; i++)
            {
                // Consider the current object apart of the pattern if the time difference of the objects is <= the threshold.
                var startTimeDiff = Math.Abs(hitObjects[i].StartTime - hitObjects[i - 1].StartTime);
                if (startTimeDiff <= 60000 / baseBpm / 4)
                {
                    patternObjects.Add(hitObjects[i]);

                    // This applies to the last HitObject. Add the pattern to the list if it is.
                    if (i != hitObjects.Count - 1 || patternObjects.Count < requiredPatternObjects)
                        continue;

                    detectedPatterns.Add(CreateJackPattern(patternObjects));
                    continue;
                } 

                // If the pattern was cut off by another object but meets the required # objects to be considered a pattern.
                if (patternObjects.Count >= requiredPatternObjects)
                    detectedPatterns.Add(CreateJackPattern(patternObjects));

                patternObjects = new List<HitObjectInfo>();
            }

            return detectedPatterns;
        }

        /// <summary>
        ///     Detects stream patterns
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <returns></returns>
        internal static List<StreamPatternInfo> DetectStreamPatterns(IReadOnlyList<HitObjectInfo> hitObjects)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Removes vibro 
        /// </summary>
        /// <returns></returns>
        internal static List<JackPatternInfo> RemoveVibroFromJacks(List<JackPatternInfo> jacks, List<JackPatternInfo> vibro)
        {
            // Remove jack patterns that intersect with vibro at the same start time.
            var intersectionPatterns = new List<JackPatternInfo>();
            jacks.ForEach(x => vibro.ForEach(y => { if (x.StartingObjectTime == y.StartingObjectTime) intersectionPatterns.Add(x); }));
            intersectionPatterns.ForEach(x => jacks.Remove(x));

            var objectsToRemove = new Dictionary<int, List<HitObjectInfo>>();

            for (var i = 0; i < jacks.Count; i++)
            {
                var detectedVibroPatterns = DetectLanePatterns(jacks[i].HitObjects, true);
                foreach (var detectedVibroPattern in detectedVibroPatterns)
                    objectsToRemove.Add(i, detectedVibroPattern.HitObjects);
            }

            foreach (var objPattern in objectsToRemove)
                jacks[objPattern.Key].HitObjects = jacks[objPattern.Key].HitObjects.Except(objPattern.Value).ToList();

            return jacks;
        }

        /// <summary>
        ///     Creates and returns an instance of a vibro pattern
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <returns></returns>
        private static JackPatternInfo CreateJackPattern(List<HitObjectInfo> hitObjects)
        {
            return new JackPatternInfo()
            {
                HitObjects = hitObjects,
                Lane = hitObjects[0].Lane,
                TotalTime = hitObjects[hitObjects.Count - 1].StartTime - hitObjects[0].StartTime,
                StartingObjectTime = hitObjects[0].StartTime
            };
        }
    }
}
