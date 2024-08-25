using System;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

public class StateTransitionEdge
{
    public delegate bool GuardDelegate(StateTransitionEdge transitionEdge, ModChartEventInstance eventInstance);

    /// <summary>
    ///     Guard from Lua
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="eventType"></param>
    /// <param name="guard"></param>
    public StateTransitionEdge(StateMachineState from, StateMachineState to, ModChartEventType eventType, Closure guard)
        : this(from, to, eventType, (edge, eventInstance) => guard.SafeCall(edge, eventInstance).ToObject<bool>())
    {
    }

    /// <summary>
    ///     Unconditional transition (no guards)
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="eventType"></param>
    public StateTransitionEdge(StateMachineState from, StateMachineState to, ModChartEventType eventType)
        : this(from, to, eventType, (_, _) => true)
    {
    }

    public StateTransitionEdge(StateMachineState from, StateMachineState to, ModChartEventType eventType,
        GuardDelegate guard)
    {
        From = from;
        To = to;
        EventType = eventType;
        Guard = guard;
        Handler = instance =>
        {
            if (Guard(this, instance))
                ModChartStateMachines.ChangeState(From, To);
        };
    }

    public StateMachineState From { get; init; }
    public StateMachineState To { get; init; }
    public ModChartEventType EventType { get; init; }
    public GuardDelegate Guard { get; init; }

    public Action<ModChartEventInstance> Handler { get; }

    public void Deconstruct(out StateMachineState from, out StateMachineState to, out ModChartEventType eventType,
        out GuardDelegate guard)
    {
        from = From;
        to = To;
        eventType = EventType;
        guard = Guard;
    }

    public override string ToString()
    {
        return $"{{{From} -- {EventType} -> {To}}}";
    }
}