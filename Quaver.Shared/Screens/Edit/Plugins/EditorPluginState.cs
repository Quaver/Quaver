using System.Linq;
using ImGuiNET;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
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
        public double SongTime { get; set; }

        [MoonSharpVisible((false))]
        private ImGuiOptions Options { get; }

        [MoonSharpVisible(false)]
        public EditorPluginState(ImGuiOptions options) => Options = options;

        /// <summary>
        ///     Pushes all styles to the current imgui context
        /// </summary>
        public void PushImguiStyle()
        {
            ImGui.PushFont(Options.Fonts.First().Context);
        }
    }
}