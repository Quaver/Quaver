using MoonSharp.Interpreter;
using Quaver.Shared.Scripting;

namespace Quaver.Shared.Screens.Edit.Plugins
{
    [MoonSharpUserData]
    public class EditorPluginState : LuaPluginState
    {
        /// <summary>
        ///     The current time in the song
        /// </summary>
        public double SongTime { get; set; }
    }
}