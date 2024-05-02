using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

/// <summary>
///     Orthogonal state machines allow you to have multiple active sub-states.
///     Upon enter, all its sub-states will be entered as well. The same goes for leaving.
/// </summary>
/// <remarks>
///     State machines, including <see cref="OrthogonalStateMachine"/>, are themselves a state, which means
///     they can be nested inside another state machine.
/// </remarks>
[MoonSharpUserData]
public class OrthogonalStateMachine : StateMachineState
{
    private readonly List<StateMachineState> _subStates = new();

    public OrthogonalStateMachine(ModChartScript script, string name = "", StateMachineState parent = default) : base(
        script, name, parent)
    {
    }

    /// <summary>
    ///     Adds a state as a child of this state machine. It will be activated when this machine is activated.
    /// </summary>
    /// <param name="state"></param>
    public override void AddSubState(StateMachineState state)
    {
        base.AddSubState(state);
        _subStates.Add(state);
    }

    public override void Update()
    {
        base.Update();
        foreach (var subState in _subStates)
        {
            subState.Update();
        }
    }

    public override IEnumerable<StateMachineState> GetActiveLeafStates()
    {
        return _subStates.SelectMany(s => s.GetActiveLeafStates());
    }

    public override IEnumerable<StateMachineState> LeafEntryStates()
    {
        return _subStates.SelectMany(s => s.LeafEntryStates());
    }

    public override IEnumerable<StateTransitionEdge> AllTransitionEdges()
    {
        return base.AllTransitionEdges().Concat(_subStates.SelectMany(s => s.AllTransitionEdges()));
    }

    public override void Enter()
    {
        base.Enter();
        foreach (var subState in _subStates)
        {
            subState.Enter();
        }
    }

    public override void Leave()
    {
        base.Leave();
        foreach (var subState in _subStates)
        {
            subState.Leave();
        }
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