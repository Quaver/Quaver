using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;

[MoonSharpUserData]
public class ModChartEvent
{
    public readonly ModChartEventType Type;
    [MoonSharpHidden] public event Action<ModChartEventType, object[]> OnInvoke;

    private readonly HashSet<Closure> _closures = new();

    public ModChartEvent(ModChartEventType type)
    {
        Type = type;
    }

    public void Add(Closure closure) => _closures.Add(closure);
    public void Remove(Closure closure) => _closures.Remove(closure);

    [MoonSharpHidden]
    public virtual void Invoke(ModChartEventType eventType, params object[] p)
    {
        OnInvoke?.Invoke(eventType, p);
        foreach (var closure in _closures)
        {
            try
            {
                closure.Call(eventType, p);
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
    }
}