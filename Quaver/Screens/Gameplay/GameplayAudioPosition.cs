using System;
using System.Collections.Generic;
using System.Text;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Database.Maps;

namespace Quaver.Screens.Gameplay
{
    public class GameplayAudioPosition
    {
        public List<SliderVelocityInfo> ScrollVelocities = new List<SliderVelocityInfo>();

        public ulong Position { get; private set; }

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
                    
                }

                // SV does not start after the last timing point
                else
                {
                    for (var j = index; j < qua.SliderVelocities.Count; j++)
                    {
                        // SV starts before the first timing point
                        if (qua.SliderVelocities[j].StartTime < qua.TimingPoints[0].StartTime)
                        {

                        }

                        // SV starts is in between two timing points
                        if (qua.SliderVelocities[j].StartTime >= qua.TimingPoints[i].StartTime
                            && qua.SliderVelocities[j].StartTime < qua.TimingPoints[i + 1].StartTime)
                        {

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
            return 0;
        }
    }
}
