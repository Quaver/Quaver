using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

public abstract class StateMachineState : IWithParent<StateMachineState>
{
    [MoonSharpHidden] public static readonly DisjointSetUnion<StateMachineState> DisjointSetUnion = new();
    public bool IsActive { get; protected set; }
    
    public ModChartScript Script { get; protected set; }

    protected StateMachineState(ModChartScript script, string name = "", StateMachineState parent = default)
    {
        Script = script;
        Name = name;
        Parent = parent;
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

    [MoonSharpHidden]
    public virtual void Enter()
    {
        if (IsActive) return;
        IsActive = true;
        if (Parent != null && !Parent.IsActive)
        {
            Parent.Enter();
        }
    }

    [MoonSharpHidden]
    public virtual void Leave()
    {
        IsActive = false;
    }
}