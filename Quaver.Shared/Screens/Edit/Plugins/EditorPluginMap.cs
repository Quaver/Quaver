using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.Shared.Screens.Edit.Plugins
{
    [MoonSharpUserData]
    public class EditorPluginMap
    {
        [MoonSharpVisible(false)]
        public Qua Map;

        /// <summary>
        ///     The game mode of the map
        /// </summary>
        public GameMode Mode { get; [MoonSharpVisible(false)] set; }

        /// <summary>
        ///     The slider velocities present in the map
        /// </summary>
        public List<SliderVelocityInfo> ScrollVelocities { get; [MoonSharpVisible(false)] set; }

        /// <summary>
        ///     The HitObjects that are currently in the map
        /// </summary>
        public List<HitObjectInfo> HitObjects { get; [MoonSharpVisible(false)] set; }

        /// <summary>
        ///     The timing points that are currently in the map
        /// </summary>
        public List<TimingPointInfo> TimingPoints { get; [MoonSharpVisible(false)] set; }

        [MoonSharpVisible(false)]
        public void SetFrameState()
        {
            Mode = Map.Mode;
            TimingPoints = Map.TimingPoints;
            ScrollVelocities = Map.SliderVelocities; // Original name was SliderVelocities but that name doesn't really make sense
            HitObjects = Map.HitObjects;
        }

        /// <summary>
        ///     Finds the most common BPM in the current map
        /// </summary>
        /// <returns></returns>
        public float GetCommonBpm() => Map.GetCommonBpm();

        /// <summary>
        ///     Gets the timing point at a particular time in the current map.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public TimingPointInfo GetTimingPointAt(double time) => Map.GetTimingPointAt(time);

        /// <summary>
        ///     Gets the scroll velocity at a particular time in the current map
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public SliderVelocityInfo GetScrollVelocityAt(double time) => Map.GetScrollVelocityAt(time);

        /// <summary>
        ///    Finds the length of a timing point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public double GetTimingPointLength(TimingPointInfo point) => Map.GetTimingPointLength(point);

    }
}
