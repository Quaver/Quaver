using MoonSharp.Interpreter;
using osu_database_reader.Components.HitObjects;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Plugins
{
    [MoonSharpUserData]
    public static class EditorPluginUtils
    {
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
    }
}