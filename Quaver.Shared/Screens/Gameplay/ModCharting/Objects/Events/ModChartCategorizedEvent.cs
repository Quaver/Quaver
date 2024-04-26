using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
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

    public override void Invoke(ModChartEventType type, params object[] p)
    {
        var specificType = type.GetSpecificType();
        var modChartEvent = this[specificType];
        modChartEvent.Invoke(type, p);
        base.Invoke(type, p);
    }
}