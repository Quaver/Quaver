using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartCategorizedEvent
{
    private readonly Dictionary<ulong, ModChartEvent> _events = new();
    public ModChartEventCategory Category { get; }
    [MoonSharpHidden] public event Action<ModChartEventType, object[]> OnInvoke;

    private readonly HashSet<Closure> _closures = new();

    public ModChartCategorizedEvent(ModChartEventCategory category)
    {
        Category = category;
    }

    public void Add(Closure closure) => _closures.Add(closure);

    public ModChartEvent this[ulong specificType]
    {
        get
        {
            _events.TryAdd(specificType, new ModChartEvent(new ModChartEventType(Category, specificType)));
            return _events[specificType];
        }
    }

    public void Remove(Closure closure) => _closures.Remove(closure);

    public void Invoke(ulong specificType, params object[] p)
    {
        var modChartEvent = this[specificType];
        var type = new ModChartEventType(Category, specificType);
        OnInvoke?.Invoke(type, p);
        foreach (var closure in _closures)
        {
            closure.Call(type, p);
        }

        modChartEvent.Invoke(p);
    }
}