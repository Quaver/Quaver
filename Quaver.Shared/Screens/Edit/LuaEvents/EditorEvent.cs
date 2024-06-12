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

    public void Add(Closure closure) => _closures.Add(closure);
    public void Add(Action<EditorEventInstance> action) => OnInvoke += action;
    public void Remove(Closure closure) => _closures.Remove(closure);
    public void Remove(Action<EditorEventInstance> action) => OnInvoke -= action;

    [MoonSharpHidden]
    public virtual void Invoke(EditorEventInstance instance)
    {
        OnInvoke?.Invoke(instance);
        foreach (var closure in _closures)
        {
            closure.SafeCall(instance);
        }
    }
}