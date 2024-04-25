using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;

[MoonSharpUserData]
public class ModChartCategorizedEvent
{
    private readonly Dictionary<int, ModChartEvent> _events = new();
    public ModChartEventType Type { get; }
    [MoonSharpHidden] public event Action<ModChartEventType, object[]> OnInvoke;

    private readonly HashSet<Closure> _closures = new();

    public ModChartCategorizedEvent(ModChartEventType type)
    {
        Type = type;
    }

    public void Add(Closure closure) => _closures.Add(closure);

    public ModChartEvent this[int specificType]
    {
        get
        {
            _events.TryAdd(specificType, new ModChartEvent(Type));
            return _events[specificType];
        }
    }

    public ModChartEvent this[ModChartEventType eventType] => this[eventType.GetSpecificType()];

    public void Remove(Closure closure) => _closures.Remove(closure);

    public void Invoke(ModChartEventType type, params object[] p)
    {
        var specificType = type.GetSpecificType();
        var modChartEvent = this[specificType];
        OnInvoke?.Invoke(type, p);
        foreach (var closure in _closures)
        {
            try
            {
                closure.Call(type, p);
            }
            catch (ScriptRuntimeException e)
            {
                Logger.Error(e.DecoratedMessage, LogType.Runtime);
            }
            catch (SyntaxErrorException e)
            {
                Logger.Error(e.DecoratedMessage, LogType.Runtime);
            }
        }

        modChartEvent.Invoke(p);
    }
}