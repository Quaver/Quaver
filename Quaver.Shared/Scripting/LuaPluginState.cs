using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Quaver.Shared.Config;
using Wobble;

#pragma warning disable CA1822
namespace Quaver.Shared.Scripting
{
    [MoonSharpUserData]
    public class LuaPluginState
    {
        // Note: Setters are purposely empty for backwards-compatibility, even if I don't expect anyone to set them.

        /// <summary>
        ///     The time elapsed between the previous and current frame
        /// </summary>
        public double DeltaTime
        {
            get => GameBase.Game.TimeSinceLastFrame;
            set => _ = value;
        }

        /// <summary>
        ///     Unix timestamp of the current time
        /// </summary>
        public long UnixTime
        {
            get => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            set => _ = value;
        }

        /// <summary>
        ///     If the plugin window is currently hovered.
        ///     This has to be set by the plugin itself
        /// </summary>
        public bool IsWindowHovered { get; set; }

        /// <summary>
        ///     Any state that the user wants to store for their plugin
        /// </summary>
        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        /// <summary>
        ///     Width and height of the current Quaver window
        /// </summary>
        public DynValue WindowSize
        {
            get =>
                DynValue.NewTable(
                    null,
                    DynValue.NewNumber(ConfigManager.WindowWidth.Value),
                    DynValue.NewNumber(ConfigManager.WindowHeight.Value)
                );
            set => _ = value;
        }

        /// <summary>
        ///     Gets a value at a particular key
        /// </summary>
        /// <param name="key"></param>
        public object GetValue(string key) => Values.GetValueOrDefault(key);

        /// <summary>
        ///     Gets a value at a particular key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fallback"></param>
        public object GetValue(string key, object fallback) => Values.GetValueOrDefault(key, fallback);

        /// <summary>
        ///     Sets a value at a particular key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, object value) => Values[key] = value;
    }
}
