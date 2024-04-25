using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;

[MoonSharpUserData]
public class ModChartDeferredEventQueue
{
    private readonly Queue<ModChartEventInstance> _eventInstances = new();
    private readonly ModChartEvents _modChartEvents;

    public ModChartDeferredEventQueue(ModChartEvents modChartEvents)
    {
        _modChartEvents = modChartEvents;
    }

    public void Enqueue(ModChartEventInstance eventInstance)
    {
        _eventInstances.Enqueue(eventInstance);
    }

    public void Clear() => _eventInstances.Clear();

    public void Dispatch()
    {
        while (_eventInstances.TryDequeue(out var eventInstance))
        {
            eventInstance.Dispatch(_modChartEvents);
        }
    }
}