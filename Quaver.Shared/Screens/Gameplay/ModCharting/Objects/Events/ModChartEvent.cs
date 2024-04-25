using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartEvent
{
    public readonly ModChartEventType Type;
    [MoonSharpHidden]
    public event Action<object[]> OnInvoke;

    private readonly HashSet<Closure> _closures = new();

    public ModChartEvent(ModChartEventType type)
    {
        Type = type;
    }

    public void Add(Closure closure) => _closures.Add(closure);
    public void Remove(Closure closure) => _closures.Remove(closure);

    [MoonSharpHidden]
    public void Invoke(params object[] p)
    {
        OnInvoke?.Invoke(p);
        foreach (var closure in _closures)
        {
            closure.Call(p.ToList());
        }
    }
}