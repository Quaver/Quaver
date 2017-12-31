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
        ///     The required amount of objects required for a vibro pattern
        /// </summary>
        private static int RequiredVibroObjects { get; } = 4;

        /// <summary>
        ///     The minimum BPM to be considered jacks
        /// </summary>
        private static int JackBaseBpm { get; } = 95;

        /// <summary>
        ///     The required amount of objects required for a vibro pattern
        /// </summary>
        private static int RequiredJackObjects { get; } = 2;

        /// <summary>
        ///     The absolute minimum bpm to be considered a chord.
        ///     Note: This is very strict, because we are trying to detect individual 
        ///     chords, so they'd have to be very very close together in start time difference
        ///     to be considered a chord.
        /// </summary>
        private static int ChordBaseBpm { get; } = 1000;

        /// <summary>
        ///     The required amount of objects for a chord pattern.
        /// </summary>
        private static int RequiredChordObjects { get; } = 2;

        /// <summary>
        ///     The minimum bpm to be considered a stream pattern
        /// </summary>
        private static int StreamBaseBpm { get; } = 120;

        /// <summary>
        ///     The required amount of objects for a stream pattern.
        /// </summary>
        private static int RequiredStreamObjects { get; } = 3;

        /// <summary>
        ///     Analyzes a map for different types of patterns
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <returns></returns>
        internal static PatternList AnalyzeMapPatterns(IReadOnlyList<HitObjectInfo> hitObjects)
        {
            // Holds the list of patterns that will be returned.
            var patterns = new PatternList()
            {
                Streams = new List<StreamPatternInfo>(),
                Jacks = new List<JackPatternInfo>(),
                Vibro = new List<JackPatternInfo>(),
                Chords = new List<ChordPatternInfo>()
            };

            // Current Stream Objects
            var currentStreamObjects = new List<HitObjectInfo>();

            // Current Chord Objects
            var currentChordObjects = new List<HitObjectInfo>();

            // Current 7 lists of jack patterns for each key lane.
            var currentJackObjects = new List<List<HitObjectInfo>>
            {
                new List<HitObjectInfo>(), new List<HitObjectInfo>(),
                new List<HitObjectInfo>(), new List<HitObjectInfo>(), 
                new List<HitObjectInfo>(), new List<HitObjectInfo>(),
                new List<HitObjectInfo>()
            };

            // Current 7 lists of vibro patterns for each key lane.
            var currentVibroObjects = new List<List<HitObjectInfo>>
            {
                new List<HitObjectInfo>(), new List<HitObjectInfo>(),
                new List<HitObjectInfo>(), new List<HitObjectInfo>(),
                new List<HitObjectInfo>(), new List<HitObjectInfo>(),
                new List<HitObjectInfo>()
            };

            // Begin analyzing stream/chord patterns
            for (var i = 1; i < hitObjects.Count; i++)
            {
                var timeDifference = Math.Abs(hitObjects[i].StartTime - hitObjects[i - 1].StartTime);

                #region streamDetection
                if (IsPatternObject(timeDifference, hitObjects[i], hitObjects[i - 1], PatternType.Stream))
                {
                    currentStreamObjects.Add(hitObjects[i - 1]);

                    // This applies to the last HitObject, add the last pattern to the list of that's the case.
                    if (i == hitObjects.Count - 1 && currentStreamObjects.Count >= RequiredStreamObjects)
                        patterns.Streams.Add(CreateStreamPattern(currentStreamObjects));
                }
                else
                {
                    // If the pattern was cut off by another object, we want to add the current one, since that
                    // is techinically still apart of the pattern (Only applicible to chords in this circumstance)
                    currentStreamObjects.Add(hitObjects[i]);

                    // If the pattern was cut off by another object but meets the required # objects to be considered a stream pattern.
                    if (currentStreamObjects.Count >= RequiredStreamObjects)
                        patterns.Streams.Add(CreateStreamPattern(currentStreamObjects));

                    currentStreamObjects = new List<HitObjectInfo>();
                }
                #endregion

                #region chordDetection
                if (IsPatternObject(timeDifference, hitObjects[i], hitObjects[i - 1], PatternType.Chord))
                {
                    currentChordObjects.Add(hitObjects[i - 1]);

                    // This applies to the last HitObject, add the last pattern to the list of that's the case.
                    if (i == hitObjects.Count - 1 && currentChordObjects.Count >= RequiredChordObjects)
                        patterns.Chords.Add(CreateChordPattern(currentChordObjects));
                }
                else
                {
                    // If the pattern was cut off by another object, we want to add the current one, since that
                    // is techinically still apart of the pattern (Only applicible to chords in this circumstance)
                    currentChordObjects.Add(hitObjects[i]);

                    // If the pattern was cut off by another object but meets the required # objects to be considered a chord pattern.
                    if (currentChordObjects.Count >= RequiredChordObjects)
                        patterns.Chords.Add(CreateChordPattern(currentChordObjects));

                    currentChordObjects = new List<HitObjectInfo>();
                }
                #endregion
            }

            #region jackVibroDetection
            // Analyze jack/vibro patterns - At this point we want to order the objects by lane, 
            // so we can accurately detect them
            // TODO: Have a variable stored for each lane which contains the start time to compare by - Potential optimization
            hitObjects = hitObjects.OrderBy(x => x.Lane).ToList();

            for (var i = 1; i < hitObjects.Count; i++)
            {
                var timeDifference = Math.Abs(hitObjects[i].StartTime - hitObjects[i - 1].StartTime);

                #region jack
                if (IsPatternObject(timeDifference, hitObjects[i], hitObjects[i - 1], PatternType.Jack))
                {
                    currentJackObjects[hitObjects[i].Lane - 1].Add(hitObjects[i - 1]);

                    // This applies to the last HitObject, add the last pattern to the list of that's the case.
                    if ((i == hitObjects.Count - 1 || hitObjects[i].Lane != hitObjects[i + 1].Lane) && currentJackObjects[hitObjects[i].Lane - 1].Count >= RequiredJackObjects)
                        patterns.Jacks.Add(CreateJackPattern(currentJackObjects[hitObjects[i].Lane - 1]));

                    #region vibro
                    // A vibro pattern will always be a jack pattern, but we want to do individual checking here while we're at it.
                    // they will be removed from jacks before returnning.
                    if (IsPatternObject(timeDifference, hitObjects[i], hitObjects[i - 1], PatternType.Vibro))
                    {
                        currentVibroObjects[hitObjects[i].Lane - 1].Add(hitObjects[i - 1]);

                        // This applies to the last HitObject or the last in the lane, add the last pattern to the list of that's the case.
                        if (i == hitObjects.Count - 1 || hitObjects[i].Lane != hitObjects[i + 1].Lane && currentVibroObjects[hitObjects[i].Lane - 1].Count >= RequiredVibroObjects)
                            patterns.Vibro.Add(CreateJackPattern(currentVibroObjects[hitObjects[i].Lane - 1]));
                            
                    }
                    else
                    {
                        // If the pattern was cut off by another object, we want to add the current one
                        currentVibroObjects[hitObjects[i].Lane - 1].Add(hitObjects[i]);

                        // If the pattern was cut off by another object but meets the required # objects to be considered a vibro pattern.
                        if (currentVibroObjects[hitObjects[i].Lane - 1].Count >= RequiredVibroObjects)
                            patterns.Vibro.Add(CreateJackPattern(currentVibroObjects[hitObjects[i].Lane - 1]));

                        currentVibroObjects[hitObjects[i].Lane - 1] = new List<HitObjectInfo>();
                    }
                    #endregion
                }
                else
                {
                    currentJackObjects[hitObjects[i].Lane - 1].Add(hitObjects[i]);

                    // If the pattern was cut off by another object but meets the required # objects to be considered a jack pattern.
                    if (currentJackObjects[hitObjects[i].Lane - 1].Count >= RequiredJackObjects)
                        patterns.Jacks.Add(CreateJackPattern(currentJackObjects[hitObjects[i].Lane - 1]));

                    currentJackObjects[hitObjects[i].Lane - 1] = new List<HitObjectInfo>();

                    // If the pattern was cut off by another object but meets the required # objects to be considered a vibro pattern.
                    if (currentVibroObjects[hitObjects[i].Lane - 1].Count >= RequiredVibroObjects)
                        patterns.Vibro.Add(CreateJackPattern(currentVibroObjects[hitObjects[i].Lane - 1]));

                    currentVibroObjects[hitObjects[i].Lane - 1] = new List<HitObjectInfo>();
                }
                #endregion
            }
            #endregion

            // Finally, remove the vibro patterns from jacks
            patterns.Jacks = RemoveVibroFromJacks(patterns.Jacks, patterns.Vibro);
            return patterns;
        }

        /// <summary>
        ///     Checks if an object is apart of a given pattern
        /// </summary>
        /// <param name="timeDiff"></param>
        /// <param name="currentObject"></param>
        /// <param name="previousObject"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsPatternObject(int timeDiff, HitObjectInfo currentObject, HitObjectInfo previousObject, PatternType type)
        {
            switch (type)
            {
                case PatternType.Stream:
                    return timeDiff <= 60000 / StreamBaseBpm / 4 && currentObject.Lane != previousObject.Lane;
                case PatternType.Jack:
                    return timeDiff <= 60000 / JackBaseBpm / 4;
                case PatternType.Chord:
                    return timeDiff <= 60000 / ChordBaseBpm / 4;
                case PatternType.Vibro:
                    return timeDiff <= 60000 / VibroBaseBpm / 4;
                default:
                    return false;
            }
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
                var detectedVibroPatterns = AnalyzeMapPatterns(jacks[i].HitObjects);
                foreach (var detectedVibroPattern in detectedVibroPatterns.Vibro)
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
        ///     Creates and returns an instance of a chord pattern
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <returns></returns>
        private static StreamPatternInfo CreateStreamPattern(List<HitObjectInfo> hitObjects)
        {
            return new StreamPatternInfo()
            {
                HitObjects = hitObjects,
                TotalTime = hitObjects[hitObjects.Count - 1].StartTime - hitObjects[0].StartTime,
                StartingObjectTime = hitObjects[0].StartTime,
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
                catch (Exception)
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
                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }
    }
}
