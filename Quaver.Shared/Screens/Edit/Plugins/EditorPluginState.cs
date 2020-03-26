using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Scripting;
using Wobble.Graphics.ImGUI;

namespace Quaver.Shared.Screens.Edit.Plugins
{
    [MoonSharpUserData]
    public class EditorPluginState : LuaPluginState
    {
        /// <summary>
        ///     The current time in the song
        /// </summary>
        public int SongTime { get; [MoonSharpVisible(false)] set; }

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

        /// <summary>
        ///     The objects that are currently selected by the user
        /// </summary>
        public List<HitObjectInfo> SelectedHitObjects { get; [MoonSharpVisible(false)] set; }

        /// <summary>
        ///     The current timing point in the map
        /// </summary>
        public TimingPointInfo CurrentTimingPoint { get; [MoonSharpVisible(false)] set; }

        /// <summary>
        ///     ImGui options used to set styles/fonts for the window
        /// </summary>
        [MoonSharpVisible((false))]
        private ImGuiOptions Options { get; }

        [MoonSharpVisible(false)]
        public EditorPluginState(ImGuiOptions options) => Options = options;

        /// <summary>
        ///     Pushes all styles to the current imgui context
        /// </summary>
        public void PushImguiStyle() => ImGui.PushFont(Options.Fonts.First().Context);
    }
}