using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Edit.Plugins;
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
        ///     Gets the current ImGui scaling.
        /// </summary>
        public float Scale => EditorPluginUtils.EditScreen.ImGuiScale;

        /// <summary>
        ///     Any state that the user wants to store for their plugin
        /// </summary>
        public Dictionary<string, DynValue> Values { get; [MoonSharpHidden] init; } = new(StringComparer.Ordinal);

        /// <summary>
        ///     Width and height of the current Quaver window
        /// </summary>
        public DynValue WindowSize
        {
            get => ImGuiRedirect.Pack(ConfigManager.WindowWidth.Value, ConfigManager.WindowHeight.Value);
            set => _ = value;
        }

        /// <summary>
        ///     Sets a value at a particular key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, DynValue value) => Values[key] = value;

        /// <summary>
        ///     Gets a value at a particular key
        /// </summary>
        /// <param name="key"></param>
        public DynValue GetValue(string key) => Values.GetValueOrDefault(key);

        /// <summary>
        ///     Gets a value at a particular key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fallback"></param>
        public DynValue GetValue(string key, DynValue fallback) => Values.GetValueOrDefault(key, fallback);

        /// <summary>
        ///     Creates the deep copy of the values.
        /// </summary>
        /// <param name="nonconvertible">The function invoked when a non-convertible element has been found.</param>
        /// <returns>The deep copy of <see cref="Values"/>.</returns>
        public Dictionary<string, DynValue> CloneValues(Converter<DynValue, DynValue> nonconvertible) =>
            Values.ToDictionary(x => x.Key, x => Clone(x.Value, nonconvertible), StringComparer.Ordinal);

        /// <summary>
        ///     Creates the deep copy of this instance.
        /// </summary>
        /// <param name="nonconvertible">The function invoked when a non-convertible element has been found.</param>
        /// <returns>The deep copy of this instance.</returns>
        public virtual LuaPluginState Clone(Converter<DynValue, DynValue> nonconvertible) =>
            new() { IsWindowHovered = IsWindowHovered, Values = CloneValues(nonconvertible) };

        private static DynValue Clone(
            DynValue value,
            Converter<DynValue, DynValue> nonconvertible,
            int depth = LuaImGui.RecursionLimit
        ) =>
            value switch
            {
                { Type: DataType.Function or DataType.Thread or DataType.TailCallRequest } => nonconvertible(value),
                { Type: DataType.Table, Table: var x } =>
                    depth > 0 ? DynValue.NewTable(Clone(x, nonconvertible, depth - 1)) : nonconvertible(value),
                { Type: DataType.Tuple, Tuple: var x } =>
                    depth > 0 ? DynValue.NewTuple(Clone(x, nonconvertible, depth - 1)) : nonconvertible(value),
                { Type: DataType.YieldRequest, YieldRequest: var x } =>
                    DynValue.NewYieldReq(Clone(x.ReturnValues, nonconvertible, depth)),
                _ => value,
            };

        private static DynValue[] Clone(DynValue[] x, Converter<DynValue, DynValue> nonconvertible, int depth) =>
            Array.ConvertAll(x, x => Clone(x, nonconvertible, depth));

        private static Table Clone(Table value, Converter<DynValue, DynValue> nonconvertible, int depth)
        {
            Table clone = new(null);

            foreach (var pair in value.Pairs)
                clone.Set(Clone(pair.Key, nonconvertible, depth - 1), Clone(pair.Value, nonconvertible, depth - 1));

            if (value.MetaTable is { } meta && meta != value)
                clone.MetaTable = Clone(meta, nonconvertible, depth - 1);

            return clone;
        }
    }
}
