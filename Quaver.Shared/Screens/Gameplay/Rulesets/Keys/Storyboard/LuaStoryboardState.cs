using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

[MoonSharpUserData]
public class LuaStoryboardState
{
    

    /// <summary>
    ///     Any state that the user wants to store for their plugin
    /// </summary>
    public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

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