using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

/// <summary>
///     A state machine with a number of sub-states and a designated entry state.
///     Upon entry, the entry state will be activated.
///     <b>Only</b> the active sub-state will be updated upon an <see cref="Update"/> call.
/// </summary>
/// <remarks>
///     State machines, including <see cref="OrthogonalStateMachine"/>, are themselves a state, which means
///     they can be nested inside another state machine.
/// </remarks>
[MoonSharpUserData]
public class StateMachine : StateMachineState
{
    private readonly List<StateMachineState> _subStates = new();
    public StateMachineState EntryState { get; set; }
    public StateMachineState ActiveState { get; protected set; }

    public StateMachine(ModChartScript script, StateMachineState entryState, string name = "",
        StateMachineState parent = default) : base(script, name,
        parent)
    {
        if (entryState != null) AddSubState(entryState);
    }

    public sealed override void AddSubState(StateMachineState state)
    {
        base.AddSubState(state);
        _subStates.Add(state);
        if (_subStates.Count == 1) EntryState = state;
    }

    public override IEnumerable<StateMachineState> GetActiveLeafStates()
    {
        return ActiveState?.GetActiveLeafStates() ?? Enumerable.Empty<StateMachineState>();
    }

    public override IEnumerable<StateMachineState> LeafEntryStates()
    {
        return EntryState.LeafEntryStates();
    }

    public override void Update()
    {
        base.Update();
        ActiveState?.Update();
    }

    public override void Enter()
    {
        base.Enter();
        ActiveState = EntryState;
        ActiveState?.Enter();
    }

    public override void Leave()
    {
        base.Leave();
        ActiveState?.Leave();
        ActiveState = null;
    }

    public override IEnumerable<StateTransitionEdge> AllTransitionEdges()
    {
        return base.AllTransitionEdges().Concat(_subStates.SelectMany(s => s.AllTransitionEdges()));
    }

    public override string DotGraphNodeName => $"cluster_{Uid}";

    public override void WriteDotGraph(TextWriter writer, bool isSubgraph)
    {
        writer.WriteLine(isSubgraph ? $"subgraph {DotGraphNodeName} {{" : $"digraph {DotGraphNodeName} {{");
        writer.WriteLine("style = solid;");
        writer.WriteLine("node [style=solid];");
        writer.WriteLine($"label = \"{Name}\";");
        foreach (var subState in _subStates)
        {
            subState.WriteDotGraph(writer, true);
        }

        writer.WriteLine($"{EntryState.DotGraphNodeName} [shape=doublecircle]");
        if (!isSubgraph)
        {
            foreach (var transitionEdge in AllTransitionEdges())
            {
                writer.WriteLine(
                    $"{transitionEdge.From.DotGraphNodeName} -> {transitionEdge.To.DotGraphNodeName} [label={transitionEdge.EventType}];");
            }
        }

        writer.WriteLine("}");
    }
}