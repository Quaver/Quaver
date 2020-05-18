using System;
using Microsoft.Xna.Framework.Input;
using MoonSharp.Interpreter;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;
using Wobble.Input;

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

        /// <summary>
        ///     Converts milliseconds to the appropriate mm:ss:ms time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string MillisecondsToTime(float time) => TimeSpan.FromMilliseconds(time).ToString(@"mm\:ss\.fff");

        public static void OpenUrl(string url, bool forceNormalBrowser = false) => BrowserHelper.OpenURL(url, forceNormalBrowser);

        public static bool IsKeyPressed(Keys k) => KeyboardManager.IsUniqueKeyPress(k);

        public static bool IsKeyReleased(Keys k) => KeyboardManager.IsUniqueKeyRelease(k);

        public static bool IsKeyDown(Keys k) => KeyboardManager.CurrentState.IsKeyDown(k);

        public static bool IsKeyUp(Keys k) => KeyboardManager.CurrentState.IsKeyUp(k);
    }
}