using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using System;
using System.Collections.Generic;
using System.Text;

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

    }
}
