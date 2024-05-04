using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

[MoonSharpUserData]
public abstract class StateMachineState : IWithParent<StateMachineState>, IDotGraphFormattable
{
    private static int _currentUid;
    protected static int GenerateUid() => _currentUid++;
    public int Uid { get; }
    [MoonSharpHidden] public static readonly DisjointSetUnion<StateMachineState> DisjointSetUnion = new();
    public bool IsActive { get; private set; }

    [MoonSharpHidden] internal List<StateTransitionEdge> OutgoingTransitions { get; } = new();

    public ModChartScript Script { get; }

    protected StateMachineState(ModChartScript script, string name = "", StateMachineState parent = default)
    {
        Uid = GenerateUid();
        Script = script;
        Name = string.IsNullOrWhiteSpace(name) ? $"AnonymousState {Uid}" : name;
        parent?.AddSubState(this);
    }

    public string Name { get; }

    /// <summary>
    ///     The parent state. Null if root
    /// </summary>
    public StateMachineState Parent { get; private set; }

    public StateMachineState Root => Parent?.Root ?? this;

    /// <summary>
    ///     Used to find the least common ancestor
    /// </summary>
    [MoonSharpHidden]
    internal StateMachineState LastLcaSearchTarget { get; set; }

    /// <summary>
    ///     If this state is the LCA, this is the direct descendent of this state on the path from this to the originalState
    /// </summary>
    [MoonSharpHidden]
    internal StateMachineState LastLcaSearchChildToSource { get; set; }


    /// <summary>
    ///     If this state is the LCA, this is the direct descendent of this state on the path from this to the targetState
    /// </summary>
    [MoonSharpHidden]
    internal StateMachineState LastLcaSearchChildToTarget { get; set; }

    [MoonSharpHidden]
    public virtual void Update()
    {
    }

    public virtual void AddSubState(StateMachineState state)
    {
        if (!(this is StateMachine || this is OrthogonalStateMachine))
            throw new InvalidOperationException($"{GetType().Name} does not allow sub-states");
        if (state.Parent == this) return;
        if (state.Parent != null)
            throw new InvalidOperationException($"Cannot add a state to more than one state machine");
        state.Parent = this;
        DisjointSetUnion.Union(this, state);
    }

    public virtual IEnumerable<StateMachineState> GetActiveLeafStates()
    {
        if (IsActive) yield return this;
    }

    public virtual IEnumerable<StateMachineState> LeafEntryStates()
    {
        yield return this;
    }

    public virtual IEnumerable<StateTransitionEdge> AllTransitionEdges()
    {
        return OutgoingTransitions;
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
        if (Parent != null && !Parent.IsActive)
        {
            Parent.Enter();
        }

        Parent?.SetActiveSubState(this);
        Logger.Debug($"[SM] {Name} entered", LogType.Runtime);
        foreach (var outgoingTransition in OutgoingTransitions)
        {
            Script.ModChartEvents.Subscribe(outgoingTransition.EventType, outgoingTransition.Handler);
        }
    }

    [MoonSharpHidden]
    public virtual void Leave()
    {
        if (!IsActive) return;
        Logger.Debug($"[SM] {Name} left", LogType.Runtime);
        IsActive = false;
        foreach (var outgoingTransition in OutgoingTransitions)
        {
            Script.ModChartEvents.Unsubscribe(outgoingTransition.EventType, outgoingTransition.Handler);
        }
    }

    public virtual bool CanEnterSubStateDirectly(StateMachineState subState)
    {
        return false;
    }

    protected virtual void SetActiveSubState(StateMachineState subState)
    {
    }

    public override string ToString()
    {
        return $"[{Name}]";
    }

    public virtual void WriteDotGraph(TextWriter writer, bool isSubgraph)
    {
        writer.WriteLine($"n{Uid} [label = \"{Name}\" color = \"{(IsActive ? "green" : "black")}\" shape=circle];");
    }

    public virtual string DotGraphNodeName => $"n{Uid}";

    public string GenerateDotGraph()
    {
        using var writer = new StringWriter();
        writer.WriteLine($"digraph {DotGraphNodeName} {{");
        writer.WriteLine($"compound = true;");
        writer.WriteLine($"nodesep = 1;");
        WriteDotGraph(writer, false);
        writer.WriteLine("}");
        return writer.ToString();
    }

    protected void WriteDotGraphEdges(TextWriter writer)
    {
        foreach (var transitionEdge in AllTransitionEdges())
        {
            var transitionStatus = ModChartStateMachines.FindLca(transitionEdge.From, transitionEdge.To, out _);
            var arrowProps = "";
            var fromName = transitionEdge.From.DotGraphNodeName;
            if (transitionEdge.From is StateMachine or OrthogonalStateMachine)
            {
                fromName = transitionEdge.From.LeafEntryStates().FirstOrDefault()?.DotGraphNodeName ?? "Unknown";
                arrowProps += $" ltail=\"{transitionEdge.From.DotGraphNodeName}\"";
            }

            var toName = transitionEdge.To.DotGraphNodeName;
            if (transitionEdge.To is StateMachine or OrthogonalStateMachine)
            {
                toName = transitionEdge.To.LeafEntryStates().FirstOrDefault()?.DotGraphNodeName ?? "Unknown";
                arrowProps += $" lhead=\"{transitionEdge.To.DotGraphNodeName}\"";
            }

            switch (transitionStatus)
            {
                case TransitionStatus.Incompatible or TransitionStatus.Unreachable:
                    arrowProps += $" color=red";
                    break;
                case TransitionStatus.Possible:
                    arrowProps += $" color=cyan";
                    break;
                case TransitionStatus.Self:
                    arrowProps += $" color=purple style=dashed";
                    break;
                case TransitionStatus.ActiveTarget or TransitionStatus.InactiveOrigin:
                    arrowProps += $" color=grey style=dashed";
                    break;
            }

            writer.WriteLine(
                $"{fromName} -> {toName} [label=\"{transitionEdge.EventType.ToFriendlyString()}\"{arrowProps}];");
        }
    }
}