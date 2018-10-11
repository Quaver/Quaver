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
        public List<SliderVelocityInfo> ScrollVelocities = new List<SliderVelocityInfo>();

        /// <summary>
        ///     Current position for Hit Objects
        /// </summary>
        public ulong Position { get; private set; }

        /// <summary>
        ///     Current SV index used for optimization when using UpdateCurrentPosition()
        /// </summary>
        private int SvIndex { get; set; } = 0;

        //todo: temp. reference from actual scroll speed variable and remove later
        public float ScrollSpeed { get; set; } = 20;

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
        }

        /// <summary>
        ///     Get Hit Object position from audio time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public ulong GetPositionFromTime(float time)
        {
            ulong curPos = 0;

            // Time starts before the first SV point
            if (time < ScrollVelocities[0].StartTime)
            {
                // todo: implment constant to calculate SV before first SV timing point
            }

            // Time starts after the first SV point
            else
            {
                // Calculate position with Time and SV
                for (var i = 0; i < ScrollVelocities.Count - 1; i++)
                {
                    if (time > ScrollVelocities[i + 1].StartTime)
                    {
                        curPos += (ulong)((ScrollVelocities[i + 1].StartTime - ScrollVelocities[i].StartTime) * ScrollVelocities[i].Multiplier * ScrollSpeed);
                    }
                    else
                    {
                        curPos += (ulong)((time - ScrollVelocities[i].StartTime) * ScrollVelocities[i].Multiplier * ScrollSpeed);
                        break;
                    }
                }

                // Add extra position if Time starts after the last SV point
                if (time >= ScrollVelocities[ScrollVelocities.Count - 1].StartTime)
                    curPos += (ulong)((time - ScrollVelocities[ScrollVelocities.Count - 1].StartTime) * ScrollVelocities[ScrollVelocities.Count - 1].Multiplier * ScrollSpeed);

            }

            return curPos;
        }

        public void UpdateCurrentPosition(float time)
        {
            // Update SV index if necessary
            if (SvIndex != ScrollVelocities.Count - 1 && time >= ScrollVelocities[SvIndex + 1].StartTime)
            {
                // todo: we might have to loop since its possible to pass multiple SV timing points that are closer together than delta-time 
                SvIndex++;
            }

            if (SvIndex >= ScrollVelocities.Count - 1)
            {

            }

            else
            {

            }

            // todo: add variables and optimization
            Position = GetPositionFromTime(time);
        }
    }
}
