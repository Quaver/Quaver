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
        ///     Detects vibro patterns 
        /// 
        ///     A "vibro" pattern can be considered as 4+ notes in succession in a given lane
        ///     at 170+ BPM (88ms or less time difference per note).
        ///     Anything lower than that would be considered a jack pattern.  
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <param name="KeyCount"></param>
        /// <returns></returns>
        internal static List<VibroPatternInfo> DetectVibroPatterns(IReadOnlyList<HitObjectInfo> hitObjects)
        {
            // Get all detected vibro patterns per lane
            var vibroPatternsLane1 = DetectLaneVibroPatterns(hitObjects.Where(x => x.Lane == 1).ToList());
            var vibroPatternsLane2 = DetectLaneVibroPatterns(hitObjects.Where(x => x.Lane == 2).ToList());
            var vibroPatternsLane3 = DetectLaneVibroPatterns(hitObjects.Where(x => x.Lane == 3).ToList());
            var vibroPatternsLane4 = DetectLaneVibroPatterns(hitObjects.Where(x => x.Lane == 4).ToList());
            var vibroPatternsLane5 = DetectLaneVibroPatterns(hitObjects.Where(x => x.Lane == 5).ToList());
            var vibroPatternsLane6 = DetectLaneVibroPatterns(hitObjects.Where(x => x.Lane == 6).ToList());
            var vibroPatternsLane7 = DetectLaneVibroPatterns(hitObjects.Where(x => x.Lane == 7).ToList());

            // Return all the vibro patterns concatenated by key lane
            var detectedPatterns = vibroPatternsLane1
                .Concat(vibroPatternsLane2)
                .Concat(vibroPatternsLane3)
                .Concat(vibroPatternsLane4)
                .Concat(vibroPatternsLane5)
                .Concat(vibroPatternsLane6)
                .Concat(vibroPatternsLane7).ToList();

            Console.WriteLine($"Lane 1 Detected Vibro Patterns: {vibroPatternsLane1.Count}");
            Console.WriteLine($"Lane 2 Detected Vibro Patterns: {vibroPatternsLane2.Count}");
            Console.WriteLine($"Lane 3 Detected Vibro Patterns: {vibroPatternsLane3.Count}");
            Console.WriteLine($"Lane 4 Detected Vibro Patterns: {vibroPatternsLane4.Count}");
            Console.WriteLine($"Lane 5 Detected Vibro Patterns: {vibroPatternsLane5.Count}");
            Console.WriteLine($"Lane 6 Detected Vibro Patterns: {vibroPatternsLane6.Count}");
            Console.WriteLine($"Lane 7 Detected Vibro Patterns: {vibroPatternsLane7.Count}");
            Console.WriteLine($"Total Detected Vibro Patterns: {detectedPatterns.Count}");
            Console.WriteLine($"Total Object Count: {hitObjects.Count}");

            return detectedPatterns;
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
            // 88ms per hit is 170BPM, so if it's an less than that, that too would be considered vibro
            const int vibroMsDiff = 88;

            // Stores the current objects in a given pattern that is being analyzed
            var currentPattern = new List<HitObjectInfo>();

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
                        if (i != hitObjects.Count - 1 || currentPattern.Count < 4)
                            continue;

                        // Add the last detected pattern to the list.
                        detectedPatterns.Add(new VibroPatternInfo
                        {
                            HitObjects = currentPattern,
                            Lane = currentPattern[0].Lane,
                            TotalTime = currentPattern[currentPattern.Count - 1].StartTime - currentPattern[0].StartTime
                        });
                    }
                        
                    // If we drop to here, this must mean that it is the end of the current pattern.
                    // So we can run a check to see how many objects are actually in the current pattern.
                    // If it's >= 4 objects, then that must mean it's a vibro pattern, so we can go ahead and 
                    // add it to the list of vibro patterns
                    else if (currentPattern.Count >= 4)
                    {
                        // Add the detected pattern to the current list
                        detectedPatterns.Add(new VibroPatternInfo
                        {
                            HitObjects = currentPattern,
                            Lane = currentPattern[0].Lane,
                            TotalTime = currentPattern[currentPattern.Count - 1].StartTime - currentPattern[0].StartTime
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
    }
}
