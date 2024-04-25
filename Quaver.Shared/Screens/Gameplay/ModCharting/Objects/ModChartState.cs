using System.Collections.Generic;
using System.Numerics;
using MoonSharp.Interpreter;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartState
{
    

    /// <summary>
    ///     Any state that the user wants to store for their plugin
    /// </summary>
    public Dictionary<string, object> Values { get; } = new();
    
    public double SongTime { get; set; }
    public TimingPointInfo CurrentTimingPoint { get; internal set; }
    
    public Vector2 WindowSize { get; internal set; }
    public long UnixTime { get; set; }

    /// <summary>
    ///     Gets a value at a particular key
    /// </summary>
    /// <param name="key"></param>
    public object GetValue(string key)
    {
        if (!Values.ContainsKey(key))
            return null;

        return Values[key];
    }

    /// <summary>
    ///     Sets a value at a particular key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetValue(string key, object value) => Values[key] = value;
}