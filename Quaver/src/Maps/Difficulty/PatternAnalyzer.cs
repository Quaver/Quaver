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
        ///     Detects vibro patterns for ther entire map
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <param name="KeyCount"></param>
        /// <returns></returns>
        internal static List<VibroPatternInfo> DetectVibroPatterns(IReadOnlyList<HitObjectInfo> hitObjects)
        {
            return DetectLaneVibroPatterns(hitObjects.Where(x => x.Lane == 1).ToList())
                .Concat(DetectLaneVibroPatterns(hitObjects.Where(x => x.Lane == 2).ToList()))
                .Concat(DetectLaneVibroPatterns(hitObjects.Where(x => x.Lane == 3).ToList()))
                .Concat(DetectLaneVibroPatterns(hitObjects.Where(x => x.Lane == 4).ToList()))
                .Concat(DetectLaneVibroPatterns(hitObjects.Where(x => x.Lane == 5).ToList()))
                .Concat(DetectLaneVibroPatterns(hitObjects.Where(x => x.Lane == 6).ToList()))
                .Concat(DetectLaneVibroPatterns(hitObjects.Where(x => x.Lane == 7).ToList()))
                .ToList();
        }

        /// <summary>
        ///     Detects jack patterns for the entire map.
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <returns></returns>
        internal static List<JackPatternInfo> DetectJackPatterns(IReadOnlyList<HitObjectInfo> hitObjects)
        {
            return DetectLaneJackPatterns(hitObjects.Where(x => x.Lane == 1).ToList())
                .Concat(DetectLaneJackPatterns(hitObjects.Where(x => x.Lane == 2).ToList()))
                .Concat(DetectLaneJackPatterns(hitObjects.Where(x => x.Lane == 3).ToList()))
                .Concat(DetectLaneJackPatterns(hitObjects.Where(x => x.Lane == 4).ToList()))
                .Concat(DetectLaneJackPatterns(hitObjects.Where(x => x.Lane == 5).ToList()))
                .Concat(DetectLaneJackPatterns(hitObjects.Where(x => x.Lane == 6).ToList()))
                .Concat(DetectLaneJackPatterns(hitObjects.Where(x => x.Lane == 7).ToList()))
                .ToList();
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
        ///     Detects vibro patterns per lane.
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <returns></returns>
        private static List<VibroPatternInfo> DetectLaneVibroPatterns(IReadOnlyList<HitObjectInfo> hitObjects)
        {
            var detectedPatterns = new List<VibroPatternInfo>();

            // The difference in milliseconds that would be detected as a vibro pattern.
            // 88ms per hit is ~165BPM, so if it's an less than that, that too would be considered vibro
            const int vibroMsDiff = 91;

            // The number of objects required to be considered a vibro pattern
            const int consideredVibroNum = 4;

            // Stores the current objects in a given pattern that is being analyzed
            var currentPattern = new List<HitObjectInfo>();

            // Begin analyzing vibro patterns
            for (var i = 1; i < hitObjects.Count; i++)
            {
                try
                {
                    var thisObject = hitObjects[i];
                    var previousObject = hitObjects[i - 1];

                    // If the start time difference of the current and previous object are 
                    // the vibro ms diff or faster, then we'll consider that apart of a vibro pattern.
                    if (Math.Abs(thisObject.StartTime - previousObject.StartTime) <= vibroMsDiff)
                    {
                        currentPattern.Add(thisObject);

                        // This only applies to the last HitObject, but run the same check
                        // to see if the pattern contains 4 or more notes.
                        if (i != hitObjects.Count - 1 || currentPattern.Count < consideredVibroNum)
                            continue;

                        // Add the last detected pattern to the list.
                        detectedPatterns.Add(new VibroPatternInfo
                        {                      
                            HitObjects = currentPattern,
                            Lane = currentPattern[0].Lane,
                            TotalTime = currentPattern[currentPattern.Count - 1].StartTime - currentPattern[0].StartTime,
                            StartingObjectTime = currentPattern[0].StartTime
                        });
                    }
                        
                    // If we drop to here, this must mean that it is the end of the current pattern.
                    // So we can run a check to see how many objects are actually in the current pattern.
                    // If it's >= 4 objects, then that must mean it's a vibro pattern, so we can go ahead and 
                    // add it to the list of vibro patterns
                    else if (currentPattern.Count >= consideredVibroNum)
                    {
                        // Add the detected pattern to the current list
                        detectedPatterns.Add(new VibroPatternInfo
                        {
                            HitObjects = currentPattern,
                            Lane = currentPattern[0].Lane,
                            TotalTime = currentPattern[currentPattern.Count - 1].StartTime - currentPattern[0].StartTime,
                            StartingObjectTime = currentPattern[0].StartTime
                        });

                        // Clear list now that there's no more patterns to add
                        currentPattern = new List<HitObjectInfo>();
                    }
                    // At the end of the pattern but it isn't necessarily considered vibro (3 or less objects)
                    else if (currentPattern.Count > 0)
                        currentPattern = new List<HitObjectInfo>();
                }
                catch (Exception) { }
            }

            return detectedPatterns;
        }

        /// <summary>
        ///     Detects jack patterns per key lane
        ///     
        ///     A Jack pattern can be considered as 2+ notes in succession in a given lane,
        ///     at a bpm range of 95-175 (159ms - 84ms object start time differences).
        /// 
        ///     One caveat about jacks is that there are certain patterns that make it harder or easier,
        ///     but this function only detects per lane and not patterns like Jumps, Hands, and Quad Jacks
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <returns></returns>
        private static List<JackPatternInfo> DetectLaneJackPatterns(IReadOnlyList<HitObjectInfo> hitObjects)
        {
            var detectedPatterns = new List<JackPatternInfo>();

            // The difference in milliseconds of the range that would be detected as a jack pattern.
            // 95BPM - 175BPM is considered jacks.
            // That's around a 84-159ms start time difference per object
            const int minJackMsDiff = 84;
            const int maxJackMsDiff = 159;

            // The number of objects required to be considered a jack pattern
            const int consideredJackNum = 2;

            // Stores the current objects in a given pattern that is being analyzed
            var currentPattern = new List<HitObjectInfo>();

            // Begin analyzing jack patterns
            for (var i = 1; i < hitObjects.Count; i++)
            {
                try
                {
                    var thisObject = hitObjects[i];
                    var previousObject = hitObjects[i - 1];

                    // If the start time difference of the current and previous object are 
                    // in range of the min and max jack millisecond difference
                    var startTimeDiff = Math.Abs(thisObject.StartTime - previousObject.StartTime);
                    if (startTimeDiff >= minJackMsDiff && startTimeDiff <= maxJackMsDiff)
                    {
                        currentPattern.Add(thisObject);

                        // This only applies to the last HitObject, but run the same check
                        // to see if the pattern contains the required amount of notes
                        if (i != hitObjects.Count - 1 || currentPattern.Count < consideredJackNum)
                            continue;

                        // Add the last detected pattern to the list.
                        detectedPatterns.Add(new JackPatternInfo
                        {
                            HitObjects = currentPattern,
                            Lane = currentPattern[0].Lane,
                            TotalTime = currentPattern[currentPattern.Count - 1].StartTime - currentPattern[0].StartTime,
                            StartingObjectTime = currentPattern[0].StartTime
                        });
                    }
                    // If we drop to here, this must mean that it is the end of the current pattern.
                    // So we can run a check to see how many objects are actually in the current pattern.
                    // If it's >= the required amount of objects, then that must mean it's a jack pattern, so we can go ahead and 
                    // add it to the list of jack patterns
                    else if (currentPattern.Count >= consideredJackNum)
                    {
                        // Add the detected pattern to the current list
                        detectedPatterns.Add(new JackPatternInfo
                        {
                            HitObjects = currentPattern,
                            Lane = currentPattern[0].Lane,
                            TotalTime = currentPattern[currentPattern.Count - 1].StartTime - currentPattern[0].StartTime,
                            StartingObjectTime = currentPattern[0].StartTime
                        });

                        // Clear list now that there's no more patterns to add
                        currentPattern = new List<HitObjectInfo>();
                    }
                    // At the end of the pattern but it isn't necessarily considered jacks
                    else if (currentPattern.Count > 0)
                        currentPattern = new List<HitObjectInfo>();
                }
                catch (Exception) { }
            }

            return detectedPatterns;
        }
    }
}
