using System;
using System.Collections.Generic;
using System.Text;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Database.Maps;

namespace Quaver.Screens.Gameplay
{
    /// <summary>
    ///     Used to position Hit Objects with Scroll Velocity relative to Audio Time.
    /// </summary>
    public class GameplayAudioPosition
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
        public GameplayAudioPosition(Qua qua)
        {
            //ScrollVelocities = qua.SliderVelocities;
            // Create SV multiplier timing points
            var index = 0;
            for (var i = 0; i < qua.TimingPoints.Count; i++)
            {
                // SV starts after the last timing point
                if (i == qua.TimingPoints.Count - 1)
                {
                    for (var j = index; j < qua.SliderVelocities.Count; j++)
                    {
                        var sv = new SliderVelocityInfo()
                        {
                            StartTime = qua.SliderVelocities[j].StartTime,
                            Multiplier = 1 //qua.SliderVelocities[j].Multiplier
                        };
                        ScrollVelocities.Add(sv);
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
                                Multiplier = 1 //qua.SliderVelocities[j].Multiplier
                            };
                            ScrollVelocities.Add(sv);
                        }

                        // SV starts is in between two timing points
                        else if (qua.SliderVelocities[j].StartTime >= qua.TimingPoints[i].StartTime
                            && qua.SliderVelocities[j].StartTime < qua.TimingPoints[i + 1].StartTime)
                        {
                            var sv = new SliderVelocityInfo()
                            {
                                StartTime = qua.SliderVelocities[j].StartTime,
                                Multiplier = 1 //qua.SliderVelocities[j].Multiplier
                            };
                            ScrollVelocities.Add(sv);
                        }

                        // Update current index if SV falls out of range for optimization
                        else
                        {
                            index = j;
                            break;
                        }
                    }
                }
            }

            // Compute for Change Points
            long position = 0;
            VelocityPositionMarkers.Add(0);

            for (var i = 0; i < ScrollVelocities.Count - 1; i++)
            {
                position += (long)((ScrollVelocities[i + 1].StartTime - ScrollVelocities[i].StartTime) * ScrollVelocities[i].Multiplier);
                VelocityPositionMarkers.Add(position);
            }
        }

        /// <summary>
        ///     Get Hit Object (End/Start) position from audio time (Unoptimized.)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public long GetPositionFromTime(float time)
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
        public long GetPositionFromTime(float time, int index)
        {
            long curPos = 0;

            // Time starts before the first SV point
            if (index == 0)
            {
                curPos = (long)((ScrollVelocities[0].StartTime - time) * ScrollVelocities[0].Multiplier);
            }

            // Time starts after the first SV point and before the last SV point
            else if (index < VelocityPositionMarkers.Count)
            {
                // Get position
                curPos += VelocityPositionMarkers[index];
                curPos += (long)((ScrollVelocities[index].StartTime - ScrollVelocities[index - 1].StartTime) * ScrollVelocities[index - 1].Multiplier);
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
        public void UpdateCurrentPosition(float audioTime)
        {
            // Update SV index if necessary
            while (SvIndex < VelocityPositionMarkers.Count && audioTime >= ScrollVelocities[SvIndex].StartTime)
            {
                SvIndex++;
            }

            // todo: add variables and optimization
            Position = GetPositionFromTime(audioTime, SvIndex);
        }
    }
}
