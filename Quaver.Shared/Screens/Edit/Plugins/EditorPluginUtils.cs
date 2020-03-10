using MoonSharp.Interpreter;
using osu_database_reader.Components.HitObjects;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Plugins
{
    [MoonSharpUserData]
    public static class EditorPluginUtils
    {
        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public static SliderVelocityInfo CreateScrollVelocity(float time, float multiplier)
        {
            var sv = new SliderVelocityInfo()
            {
                StartTime = time,
                Multiplier = multiplier,
                IsEditableInLuaScript = true
            };

            return sv;
        }

        /// <summary>
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="lane"></param>
        /// <param name="endTime"></param>
        /// <param name="hitsounds"></param>
        /// <returns></returns>
        public static HitObjectInfo CreateHitObject(int startTime, int lane, int endTime = 0, HitSounds hitsounds = 0)
        {
            var ho = new HitObjectInfo()
            {
                StartTime = startTime,
                Lane = lane,
                EndTime = endTime,
                HitSound = hitsounds
            };

            return ho;
        }

        /// <summary>
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="bpm"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public static TimingPointInfo CreateTimingPoint(int startTime, int bpm, TimeSignature signature = TimeSignature.Quadruple)
        {
            var tp = new TimingPointInfo()
            {
                StartTime = startTime,
                Bpm = bpm,
                Signature = signature
            };

            return tp;
        }
    }
}