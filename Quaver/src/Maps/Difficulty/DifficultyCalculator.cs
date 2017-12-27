using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Maps.Difficulty.Structures;

namespace Quaver.Maps.Difficulty
{
    internal static class DifficultyCalculator
    {
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
        internal static List<HitObjectInfo> RemoveArtificialDensity(Qua qua)
        {
            // First sort the Qua by StartTime so we can accurately
            // find information about the map 
            qua.Sort();

            // Start removing artificial density patterns.
            for (var i = 0; i < qua.HitObjects.Count; i++)
            {
                // We only want to run this on LN patterns, so skip normal notes.
                if (qua.HitObjects[i].EndTime == 0) continue;
                
                // Find the current timing point and next long note in the map.
                var currentTimingPoint = FindHitObjectTimingPoint(qua.HitObjects[i], qua.TimingPoints);
                var nextLongNote = FindNextLongNote(qua.HitObjects, i);

                if (nextLongNote == null || currentTimingPoint == null) continue;

                // Artificial Density Flags
                var shortStartTimes = false; // If the LNs have an extremely short start time difference compared to the BPM snap. (>= 1/8 snap)
                var shortHoldTime = false; // If the LN has a short hold time to the point where it is 300-able by tapping

                // Find the amount of milliseconds per beat this BPM has
                var millisecondsPerBeat = 60000 / currentTimingPoint.Bpm;

                // Check if the snap distance of the two object's StartTime difference is too short.
                // We consider them too short if the next object starts at a 1/8th note or smaller later.
                if (millisecondsPerBeat / (nextLongNote.StartTime - qua.HitObjects[i].StartTime) >= 8) shortStartTimes = true;

                // If the LN's hold time lasts for 1/8th of the beat or less, then that's considered artificial density.
                if (Math.Round(millisecondsPerBeat / (qua.HitObjects[i].EndTime - qua.HitObjects[i].StartTime), 0) >= 8) shortHoldTime = true;

                // If no artificial density flags were triggered for this pattern, then continue to the next object.
                if (!shortStartTimes && !shortHoldTime) continue;

                // Otherwise make the LN a normal note.
                qua.HitObjects[i].EndTime = 0;

                //Console.WriteLine($"Obj: {qua.HitObjects[i].StartTime} | TP: {currentTimingPoint.StartTime} | Next LN: {nextLongNote.StartTime} | Short Start: {shortStartTimes} | Short Hold: {shortHoldTime}");
            }

            return qua.HitObjects;
        }

        /// <summary>
        ///     Calculates the difficulty of vibro patterns.
        ///     
        ///     Vibro difficulty should be based on the following:
        ///         - Speed
        ///         - Stamina
        ///         - Control (?)
        /// 
        ///     There needs to be some sort of line that compares vibro skill to jack & stream skill.
        ///     Since vibro is an entirely different skill in itself, 
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <returns></returns>
        internal static double CalculateVibroDifficulty(List<VibroPatternInfo> patterns)
        {
            return 0;
        }

        /// <summary>
        ///     Calculates the difficulty of jack patterns
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <returns></returns>
        internal static double CalculateJackDifficulty(IReadOnlyList<HitObjectInfo> hitObjects)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Calculates the difficulty of stream patterns
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <returns></returns>
        internal static double CalculateStreamDifficulty(IReadOnlyList<HitObjectInfo> hitObjects)
        {
            throw new NotImplementedException();
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
