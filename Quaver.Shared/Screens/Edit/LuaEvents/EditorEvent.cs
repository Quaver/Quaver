using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Quaver.Shared.Scripting;

namespace Quaver.Shared.Screens.Edit.LuaEvents;

[MoonSharpUserData]
public class EditorEvent
{
    [MoonSharpHidden] public event Action<EditorEventInstance> OnInvoke;

    private readonly HashSet<Closure> _closures = new();

    /// <summary>
    ///     Make the closure be called when this event is triggered
    /// </summary>
    /// <param name="closure">A Lua function(instance: <see cref="EditorEventInstance"/>)</param>
    public void Add(Closure closure) => _closures.Add(closure);

    /// <summary>
    /// </summary>
    /// <param name="action"></param>
    public void Add(Action<EditorEventInstance> action) => OnInvoke += action;

    /// <summary>
    ///     Make the closure no longer be called when this event is triggered
    /// </summary>
    /// <param name="closure">A Lua function(instance: <see cref="EditorEventInstance"/>)</param>
    public void Remove(Closure closure) => _closures.Remove(closure);

    /// <summary>
    /// </summary>
    /// <param name="action"></param>
    public void Remove(Action<EditorEventInstance> action) => OnInvoke -= action;

    /// <summary>
    ///     Invokes the event with an instance
    /// </summary>
    /// <param name="instance">The arguments of the event</param>
    [MoonSharpHidden]
    public void Invoke(EditorEventInstance instance)
    {
        OnInvoke?.Invoke(instance);
        foreach (var closure in _closures)
        {
            closure.SafeCall(instance);
        }
    }
}