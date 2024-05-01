using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;

[MoonSharpUserData]
public class ModChartEvent
{
    public readonly ModChartEventType Type;
    [MoonSharpHidden] public event Action<ModChartEventInstance> OnInvoke;

    private readonly HashSet<Closure> _closures = new();

    public ModChartEvent(ModChartEventType type)
    {
        Type = type;
    }

    public void Add(Closure closure) => _closures.Add(closure);
    public void Remove(Closure closure) => _closures.Remove(closure);

    [MoonSharpHidden]
    public virtual void Invoke(ModChartEventInstance instance)
    {
        OnInvoke?.Invoke(instance);
        foreach (var closure in _closures)
        {
            closure.SafeCall(instance);
        }
    }
}