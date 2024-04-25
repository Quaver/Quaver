using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

public abstract class StateMachineState : IWithParent<StateMachineState>
{
    [MoonSharpHidden]
    public static readonly DisjointSetUnion<StateMachineState> DisjointSetUnion = new();

    protected StateMachineState(string name = "", StateMachineState parent = default)
    {
        Name = name;
        Parent = parent;
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

    public void Update()
    {
        Parent?.Update();
        OnUpdate();
    }

    [MoonSharpHidden]
    public abstract void OnUpdate();

    [MoonSharpHidden]
    public abstract void OnEnter();

    [MoonSharpHidden]
    public abstract void OnLeave();
}