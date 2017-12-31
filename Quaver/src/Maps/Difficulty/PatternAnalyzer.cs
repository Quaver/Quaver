using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Maps.Difficulty.Patterns;

namespace Quaver.Maps.Difficulty
{
    internal static class PatternAnalyzer
    {
        /// <summary>
        ///     The minimum BPM to be considered vibro
        /// </summary>
        private static int VibroBaseBpm { get; } = 160;

        /// <summary>
        ///     The minimum BPM to be considered jacks
        /// </summary>
        private static int JackBaseBpm { get; } = 95;

        /// <summary>
        ///     The absolute minimum bpm to be considered a chord.
        ///     Note: This is very strict, because we are trying to detect individual 
        ///     chords, so they'd have to be very very close together in start time difference
        ///     to be considered a chord.
        /// </summary>
        private static int ChordBaseBpm { get; } = 1000;

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
        ///     Detects vibro/jack patterns per key lane
        ///     
        ///     A vibro pattern can be considered as 4 or more notes in succession for a lane
        ///     with the objects having a 1/4th snap millisecond difference of a base bpm
        /// 
        ///     A jack pattern can be considered as 2 or more notes in succession for a lane
        ///     with the objects having a 1/4th snap millisecond difference of a base bpm.
        /// 
        ///     A jack pattern may include vibro patterns, however they are to be removed when
        ///     calculating difficulty: See: RemoveVibroFromJacks()
        /// <param name="hitObjects"></param>
        /// <returns></returns>
        /// </summary>
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
        ///     Detects chord patterns
        ///
        ///     A chord pattern is defined as 2 or more notes that can register as 100% accuracy hits if
        ///     pressed at the same time.
        ///     
        ///     This usually 99BPM and below
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <returns></returns>
        internal static List<ChordPatternInfo> DetectChordPatterns(IReadOnlyList<HitObjectInfo> hitObjects)
        {
            var requiredPatternObjects = 2; // The amount of objects required to be considered a pattern
            var detectedPatterns = new List<ChordPatternInfo>(); // Detected patterns
            var patternObjects = new List<HitObjectInfo>(); // The current pattern's objects

            // Begin analyzing patterns
            for (var i = 1; i < hitObjects.Count; i++)
            {
                // Consider the current object apart of the pattern if the time difference of the objects is <= the threshold.
                var startTimeDiff = Math.Abs(hitObjects[i].StartTime - hitObjects[i - 1].StartTime);
                if (startTimeDiff <= 60000 / ChordBaseBpm / 4)
                {
                    patternObjects.Add(hitObjects[i - 1]);

                    // This applies to the last HitObject. Add the pattern to the list if it is.
                    if (i != hitObjects.Count - 1 || patternObjects.Count < requiredPatternObjects)
                        continue;

                    detectedPatterns.Add(CreateChordPattern(patternObjects));
                    continue;
                }

                // If the pattern was cut off by another object, we want to add the current one, since that
                // is techinically still apart of the pattern (Only applicible to chords in this circumstance)
                patternObjects.Add(hitObjects[i]);

                // If the pattern was cut off by another object but meets the required # objects to be considered a pattern.
                if (patternObjects.Count >= requiredPatternObjects)
                    detectedPatterns.Add(CreateChordPattern(patternObjects));

                patternObjects = new List<HitObjectInfo>();
            }

            return detectedPatterns;
        }

        /// <summary>
        ///     Removes any HitObjects that are apart of vibro patterns in the jack patterns.
        ///     It uses the same vibro detection, so we're just getting rid of duplicates here.
        /// </summary>
        /// <returns></returns>
        internal static List<JackPatternInfo> RemoveVibroFromJacks(List<JackPatternInfo> jacks, List<JackPatternInfo> vibro)
        {
            // Remove jack patterns that intersect with vibro at the same start time.
            var intersectionPatterns = new List<JackPatternInfo>();
            jacks.ForEach(x => vibro.ForEach(y => { if (x.StartingObjectTime == y.StartingObjectTime) intersectionPatterns.Add(x); }));
            intersectionPatterns.ForEach(x => jacks.Remove(x));

            // Tuple stored as, <index of jack patterns, list of objects>
            var objectsToRemove = new List<Tuple<int, List<HitObjectInfo>>>();

            for (var i = 0; i < jacks.Count; i++)
            {
                var detectedVibroPatterns = DetectLanePatterns(jacks[i].HitObjects, true);
                foreach (var detectedVibroPattern in detectedVibroPatterns)
                    objectsToRemove.Add(Tuple.Create(i, detectedVibroPattern.HitObjects));
            }

            foreach (var objPattern in objectsToRemove)
                jacks[objPattern.Item1].HitObjects = jacks[objPattern.Item1].HitObjects.Except(objPattern.Item2).ToList();

            return jacks;
        }

        /// <summary>
        ///     Creates and returns an instance of a jack pattern
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

        /// <summary>
        ///     Creates and returns an instance of a chord pattern
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <returns></returns>
        private static ChordPatternInfo CreateChordPattern(List<HitObjectInfo> hitObjects)
        {
            ChordType chordType;

            switch (hitObjects.Count)
            {
                case 4:
                    chordType = ChordType.Quad;
                    break;
                case 3:
                    chordType = ChordType.Hand;
                    break;
                case 2:
                    chordType = ChordType.Jump;
                    break;
                default:
                    chordType = ChordType.FivePlus;
                    break;
            }

            return new ChordPatternInfo()
            {
                HitObjects = hitObjects,
                TotalTime = hitObjects[hitObjects.Count - 1].StartTime - hitObjects[0].StartTime,
                StartingObjectTime = hitObjects[0].StartTime,
                ChordType = chordType      
            };
        }

        /// <summary>
        ///     Removes any patterns that contain artificial density
        ///     This is essentially a C# port of Swan's ManiaStarReducer.
        /// 
        ///     Essentially it goes through the map and finds patterns that contain
        ///     artificial density and replaces them with a normal note.
        /// 
        ///     You can read more about it at the original repository:
        ///     https://github.com/Swan/ManiaStarReducer
        /// </summary>
        /// <param name="qua"></param>
        /// <returns></returns>
        internal static List<HitObjectInfo> RemoveArtificialDensity(List<HitObjectInfo> hitObjects, List<TimingPointInfo> timingPoints)
        {
            // First sort the Qua by StartTime so we can accurately
            // find information about the map 
            hitObjects = hitObjects.OrderBy(x => x.StartTime).ToList();
            timingPoints = timingPoints.OrderBy(x => x.StartTime).ToList();

            // Start removing artificial density patterns.
            for (var i = 0; i < hitObjects.Count; i++)
            {
                // We only want to run this on LN patterns, so skip normal notes.
                if (hitObjects[i].EndTime == 0) continue;

                // Find the current timing point and next long note in the map.
                var currentTimingPoint = FindHitObjectTimingPoint(hitObjects[i], timingPoints);
                var nextLongNote = FindNextLongNote(hitObjects, i);

                if (nextLongNote == null || currentTimingPoint == null) continue;

                // Artificial Density Flags
                var shortStartTimes = false; // If the LNs have an extremely short start time difference compared to the BPM snap. (>= 1/8 snap)
                var shortHoldTime = false; // If the LN has a short hold time to the point where it is 300-able by tapping

                // Find the amount of milliseconds per beat this BPM has
                var millisecondsPerBeat = 60000 / currentTimingPoint.Bpm;

                // Check if the snap distance of the two object's StartTime difference is too short.
                // We consider them too short if the next object starts at a 1/8th note or smaller later.
                if (millisecondsPerBeat / (nextLongNote.StartTime - hitObjects[i].StartTime) >= 8) shortStartTimes = true;

                // If the LN's hold time lasts for 1/8th of the beat or less, then that's considered artificial density.
                if (Math.Round(millisecondsPerBeat / (hitObjects[i].EndTime - hitObjects[i].StartTime), 0) >= 8) shortHoldTime = true;

                // If no artificial density flags were triggered for this pattern, then continue to the next object.
                if (!shortStartTimes && !shortHoldTime) continue;

                // Otherwise make the LN a normal note.
                hitObjects[i].EndTime = 0;

                //Console.WriteLine($"Obj: {hitObjects[i].StartTime} | TP: {currentTimingPoint.StartTime} | Next LN: {nextLongNote.StartTime} | Short Start: {shortStartTimes} | Short Hold: {shortHoldTime}");
            }

            return hitObjects;
        }

        /// <summary>
        ///     Searches through provided timing points and finds the one the 
        ///     HitObject is in range of. 
        /// </summary>
        /// <param name="hitObject"></param>
        /// <param name="timingPoints"></param>
        /// <returns></returns>
        private static TimingPointInfo FindHitObjectTimingPoint(HitObjectInfo hitObject, IReadOnlyList<TimingPointInfo> timingPoints)
        {
            for (var j = 0; j < timingPoints.Count; j++)
            {
                try
                {
                    var hitObjectStart = hitObject.StartTime;
                    var timingPointStart = timingPoints[j].StartTime;
                    var nextTimingPointStart = timingPoints[j + 1].StartTime;

                    if (hitObjectStart >= timingPointStart && hitObjectStart < nextTimingPointStart)
                        return timingPoints[j];

                }
                catch (Exception e)
                {
                    return timingPoints[j];
                }
            }

            return null;
        }

        /// <summary>
        ///     Finds the next long note in a map given a starting index.
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <param name="startingIndex"></param>
        /// <returns></returns>
        private static HitObjectInfo FindNextLongNote(IReadOnlyList<HitObjectInfo> hitObjects, int startingIndex)
        {
            for (var i = startingIndex + 1; i < hitObjects.Count; i++)
            {
                try
                {
                    if (hitObjects[i].EndTime > 0)
                        return hitObjects[i];
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            return null;
        }
    }
}
