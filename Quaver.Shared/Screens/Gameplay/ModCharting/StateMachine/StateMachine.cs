using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

public abstract class StateMachine : StateMachineState
{
    public readonly List<StateMachineState> SubStates = new();
    public StateMachineState EntryState { get; set; }

    public void AddState(StateMachineState state)
    {
        state.Parent = this;
        SubStates.Add(state);
        DisjointSetUnion.Union(this, state);
    }
}