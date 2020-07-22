using System;
using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Database.Maps;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit.AutoMod
{
    public class AutoModTaskHitObjects : AutoModTask
    {
        private Qua Qua { get; }

        /// <summary>
        ///     The amount of time in milliseconds where two notes would be considered overlapping/too close
        /// </summary>
        private const int OverlapThreshold = 10;

        /// <summary>
        ///     The amount of time in milliseconds where a long note would be considered too short.
        /// </summary>
        private const int ShortLongNoteThreshold = 36;

        public AutoModTaskHitObjects(Map map, Qua qua) : base(map) => Qua = qua;

        public override void Run()
        {
            var previousColumnNote = new List<HitObjectInfo>();

            for (var i = 0; i < Qua.GetKeyCount(); i++)
                previousColumnNote.Add(null);

            for (var i = 0; i < Qua.HitObjects.Count; i++)
            {
                var hitObject = Qua.HitObjects[i];
                var laneIndex = hitObject.Lane - 1;

                // Long note is too short
                if (hitObject.IsLongNote && Math.Abs(hitObject.EndTime - hitObject.StartTime) <= ShortLongNoteThreshold)
                    Logger.Debug($"Detected too short long note @ {hitObject.StartTime}", LogType.Runtime, false);

                // Object starts before the audio begins
                if (hitObject.StartTime < 0 || hitObject.IsLongNote && hitObject.EndTime < 0)
                    Logger.Debug($"Detected object that starts before 0 @ {hitObject.StartTime}|{hitObject.Lane}", LogType.Runtime, false);

                // Start at the second object when checking for overlaps
                if (i == 0)
                {
                    previousColumnNote[laneIndex] = hitObject;
                    continue;
                }

                var previousObject = previousColumnNote[laneIndex];

                if (previousObject != null)
                {
                    // Check for overlaps with the previous note in the column
                    if (Math.Abs(hitObject.StartTime - previousObject.StartTime) <= OverlapThreshold)
                        Logger.Debug($"Found overlap @ {hitObject.StartTime} and {previousObject.StartTime}", LogType.Runtime, false);

                    // Check for notes that overlap long notes
                    if (previousObject.IsLongNote && hitObject.StartTime >= previousObject.StartTime && hitObject.StartTime <= previousObject.EndTime)
                    {
                        Logger.Debug($"Found object overlapping long note {hitObject.StartTime} | " +
                                     $"{previousObject.StartTime} / {previousObject.EndTime}", LogType.Runtime, false);
                    }
                }

                previousColumnNote[laneIndex] = hitObject;
            }
        }
    }
}