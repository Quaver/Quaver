using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

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

    public StateMachine(StateMachineState entryState, string name = "", StateMachineState parent = default) : base(name,
        parent)
    {
        AddSubState(entryState);
        EntryState = entryState;
    }

    public sealed override void AddSubState(StateMachineState state)
    {
        base.AddSubState(state);
        _subStates.Add(state);
    }

    public override IEnumerable<StateMachineState> GetActiveLeafStates()
    {
        return ActiveState?.GetActiveLeafStates() ?? Enumerable.Empty<StateMachineState>();
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
}