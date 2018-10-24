using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Modifiers;

namespace Quaver.Screens.Gameplay
{
    /// <summary>
    ///     Used to position Hit Objects to Scroll Velocity (SV) relative to Audio Time.
    /// </summary>
    public class GameplayTrackManager
    {
        /// <summary>
        ///     List of slider velocities used for the current map
        /// </summary>
        public List<SliderVelocityInfo> ScrollVelocities { get; set; } = new List<SliderVelocityInfo>();

        /// <summary>
        ///     List of added hit object positions calculated from SV. Used for optimization
        /// </summary>
        public List<long> VelocityPositionMarkers { get; set; } = new List<long>();

        /// <summary>
        ///     Current position for Hit Objects
        /// </summary>
        public long Position { get; private set; }

        /// <summary>
        ///     Current SV index used for optimization when using UpdateCurrentPosition()
        ///     Default value is 0. "0" means that Current time has not passed first SV point yet.
        /// </summary>
        private int SvIndex { get; set; } = 0;

        /// <summary>
        ///     Generate Hit Object Position from .Qua Scroll Velocities
        /// </summary>
        /// <param name="qua"></param>
        public GameplayTrackManager(Qua qua)
        {
            // Find average bpm
            var commonBpm = qua.GetCommonBpm();
            //Console.Out.WriteLine("common BPM: " + commonBpm);

            // Create SV multiplier timing points
            var index = 0;
            for (var i = 0; i < qua.TimingPoints.Count; i++)
            {
                var svFound = false;

                // SV starts after the last timing point
                if (i == qua.TimingPoints.Count - 1)
                {
                    for (var j = index; j < qua.SliderVelocities.Count; j++)
                    {
                        var sv = new SliderVelocityInfo()
                        {
                            StartTime = qua.SliderVelocities[j].StartTime,
                            Multiplier = qua.SliderVelocities[j].Multiplier * (float)(qua.TimingPoints[i].Bpm / commonBpm)
                        };
                        ScrollVelocities.Add(sv);

                        // Toggle SvFound if inheriting point is overlapping timing point
                        if (Math.Abs(sv.StartTime - qua.TimingPoints[i].StartTime) < 1)
                            svFound = true;
                    }
                }

                // SV does not start after the last timing point
                else
                {
                    for (var j = index; j < qua.SliderVelocities.Count; j++)
                    {
                        // SV starts before the first timing point
                        if (qua.SliderVelocities[j].StartTime < qua.TimingPoints[0].StartTime)
                        {
                            var sv = new SliderVelocityInfo()
                            {
                                StartTime = qua.SliderVelocities[j].StartTime,
                                Multiplier = qua.SliderVelocities[j].Multiplier * (float)(qua.TimingPoints[0].Bpm / commonBpm)
                            };
                            ScrollVelocities.Add(sv);

                            // Toggle SvFound if inheriting point is overlapping timing point
                            if (Math.Abs(sv.StartTime - qua.TimingPoints[0].StartTime) < 1)
                                svFound = true;
                        }

                        // SV start is in between two timing points
                        else if (qua.SliderVelocities[j].StartTime >= qua.TimingPoints[i].StartTime
                            && qua.SliderVelocities[j].StartTime < qua.TimingPoints[i + 1].StartTime)
                        {
                            var sv = new SliderVelocityInfo()
                            {
                                StartTime = qua.SliderVelocities[j].StartTime,
                                Multiplier = qua.SliderVelocities[j].Multiplier * (float)(qua.TimingPoints[i].Bpm / commonBpm)
                            };
                            ScrollVelocities.Add(sv);

                            // Toggle SvFound if inheriting point is overlapping timing point
                            if (Math.Abs(sv.StartTime - qua.TimingPoints[i].StartTime) < 1)
                                svFound = true;
                        }

                        // Update current index if SV falls out of range for optimization
                        else
                        {
                            index = j;
                            break;
                        }
                    }
                }

                // Create BPM SV if no inheriting point is overlapping the current timing point
                if (!svFound)
                {
                    var sv = new SliderVelocityInfo()
                    {
                        StartTime = qua.TimingPoints[i].StartTime,
                        Multiplier = (float)(qua.TimingPoints[i].Bpm / commonBpm)
                    };
                    ScrollVelocities.Add(sv);
                }
            }

            // Sort list
            ScrollVelocities = ScrollVelocities.OrderBy(o => o.StartTime).ToList();

            // Compute for Change Points
            var position = (long)(ScrollVelocities[0].StartTime * ScrollVelocities[0].Multiplier);
            VelocityPositionMarkers.Add(position);

            for (var i = 1; i < ScrollVelocities.Count; i++)
            {
                position += (long)((ScrollVelocities[i].StartTime - ScrollVelocities[i - 1].StartTime) * ScrollVelocities[i - 1].Multiplier);
                VelocityPositionMarkers.Add(position);
            }
        }

        /// <summary>
        ///     Get Hit Object (End/Start) position from audio time (Unoptimized.)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public long GetPositionFromTime(double time)
        {
            long curPos = 0;

            if (time < ScrollVelocities[0].StartTime)
            {
                curPos = GetPositionFromTime(time, 0);
            }
            else if (time >= ScrollVelocities[ScrollVelocities.Count - 1].StartTime)
            {
                curPos = GetPositionFromTime(time, ScrollVelocities.Count);
            }
            else
            {
                // Get index
                for(var i = 0; i < ScrollVelocities.Count; i++)
                {
                    if (time < ScrollVelocities[i].StartTime)
                    {
                        curPos = GetPositionFromTime(time, i);
                        break;
                    }
                }
            }

            return curPos;
        }

        /// <summary>
        ///     Get Hit Object (End/Start) position from audio time and SV Index.
        ///     Index used for optimization
        /// </summary>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public long GetPositionFromTime(double time, int index)
        {
            // NoSV Modifier is toggled on
            if (ModManager.IsActivated(ModIdentifier.NoSliderVelocity))
                return (long)time;

            // Continue if SV is enabled
            long curPos = 0;

            // Time starts before the first SV point
            if (index == 0)
            {
                curPos = (long)(time * ScrollVelocities[0].Multiplier);
            }

            // Time starts after the first SV point and before the last SV point
            else if (index < VelocityPositionMarkers.Count)
            {
                // Reference the correct ScrollVelocities index by subracting 1
                index--;

                // Get position
                curPos += VelocityPositionMarkers[index];
                curPos += (long)((time - ScrollVelocities[index].StartTime) * ScrollVelocities[index].Multiplier);
            }

            // Time starts after the last SV point
            else
            {
                // Throw exception if index exceeds list size for some reason
                if (index > VelocityPositionMarkers.Count)
                    throw new Exception("index exceeds Velocity Position Marker List Size");

                // Reference the correct ScrollVelocities index by subracting 1
                index--;

                // Get position
                curPos += VelocityPositionMarkers[index];
                curPos += (long)((time - ScrollVelocities[index].StartTime) * ScrollVelocities[index].Multiplier);
            }

            return curPos;
        }

        /// <summary>
        ///     Update Current position of the hit objects
        /// </summary>
        /// <param name="audioTime"></param>
        public void UpdateCurrentPosition(double audioTime)
        {
            // Use necessary hit offset
            audioTime -= ConfigManager.GlobalAudioOffset.Value + MapManager.Selected.Value.LocalOffset;

            // Update SV index if necessary
            while (SvIndex < ScrollVelocities.Count && audioTime >= ScrollVelocities[SvIndex].StartTime)
            {
                SvIndex++;
            }
            Position = GetPositionFromTime(audioTime, SvIndex);
        }
    }
}
