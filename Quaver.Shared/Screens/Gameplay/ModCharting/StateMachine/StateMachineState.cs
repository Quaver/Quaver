using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

[MoonSharpUserData]
public abstract class StateMachineState : IWithParent<StateMachineState>
{
    private static int _currentUid;
    protected static int GenerateUid() => _currentUid++;
    public int Uid { get; }
    [MoonSharpHidden] public static readonly DisjointSetUnion<StateMachineState> DisjointSetUnion = new();
    public bool IsActive { get; protected set; }

    [MoonSharpHidden] internal List<StateTransitionEdge> OutgoingTransitions { get; } = new();

    public ModChartScript Script { get; protected set; }

    protected StateMachineState(ModChartScript script, string name = "", StateMachineState parent = default)
    {
        Uid = GenerateUid();
        Script = script;
        Parent = parent;
        Name = string.IsNullOrWhiteSpace(name) ? $"AnonymousState {Uid}" : name;
        Parent?.AddSubState(this);
    }

    public string Name { get; }

    /// <summary>
    ///     The parent state. Null if root
    /// </summary>
    public StateMachineState Parent { get; set; }

    /// <summary>
    ///     Used to find the least common ancestor
    /// </summary>
    [MoonSharpHidden]
    internal StateMachineState LastLcaSearchTarget { get; set; }

    /// <summary>
    ///     If this state is the LCA, this is the direct descendent of this state on the path from this to the originalState
    /// </summary>
    [MoonSharpHidden]
    internal StateMachineState LastLcaSearchChild { get; set; }

    [MoonSharpHidden]
    public virtual void Update()
    {
    }

    public virtual void AddSubState(StateMachineState state)
    {
        if (!(this is StateMachine || this is OrthogonalStateMachine))
            throw new InvalidOperationException($"{GetType().Name} does not allow sub-states");
        if (state.Parent != null)
            throw new InvalidOperationException($"Cannot add a state to more than one state machine");
        state.Parent = this;
        DisjointSetUnion.Union(this, state);
    }

    public virtual IEnumerable<StateMachineState> GetActiveLeafStates()
    {
        if (IsActive) yield return this;
    }

    public void AddTransition(StateTransitionEdge transitionEdge)
    {
        if (transitionEdge.From != this)
            throw new InvalidOperationException($"Adding transition {transitionEdge} to {this}");
        OutgoingTransitions.Add(transitionEdge);
    }

    public void AddTransition(StateMachineState targetState, ModChartEventType eventType,
        StateTransitionEdge.GuardDelegate guard)
    {
        AddTransition(new StateTransitionEdge(this, targetState, eventType, guard));
    }

    public void AddTransition(StateMachineState targetState, ModChartEventType eventType)
    {
        AddTransition(new StateTransitionEdge(this, targetState, eventType));
    }

    [MoonSharpHidden]
    public virtual void Enter()
    {
        if (IsActive) return;
        IsActive = true;
        foreach (var outgoingTransition in OutgoingTransitions)
        {
            Script.ModChartEvents.Subscribe(outgoingTransition.EventType, outgoingTransition.Handler);
        }

        if (Parent != null && !Parent.IsActive)
        {
            Parent.Enter();
        }
    }

    [MoonSharpHidden]
    public virtual void Leave()
    {
        if (!IsActive) return;
        IsActive = false;
        foreach (var outgoingTransition in OutgoingTransitions)
        {
            Script.ModChartEvents.Unsubscribe(outgoingTransition.EventType, outgoingTransition.Handler);
        }
    }

    public override string ToString()
    {
        return $"[{Name}]";
    }
}