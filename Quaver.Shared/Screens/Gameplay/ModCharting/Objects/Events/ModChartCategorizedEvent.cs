using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;

[MoonSharpUserData]
public class ModChartCategorizedEvent : ModChartEvent
{
    private readonly Dictionary<int, ModChartEvent> _events = new();

    public ModChartCategorizedEvent(ModChartEventType type) : base(type)
    {
    }

    public ModChartEvent this[int specificType]
    {
        get
        {
            _events.TryAdd(specificType, new ModChartEvent(Type));
            return _events[specificType];
        }
    }

    public ModChartEvent this[ModChartEventType eventType] => this[eventType.GetSpecificType()];

    public override void Invoke(ModChartEventInstance instance)
    {
        var specificType = instance.EventType.GetSpecificType();
        var modChartEvent = this[specificType];
        modChartEvent.Invoke(instance);
        base.Invoke(instance);
    }
}