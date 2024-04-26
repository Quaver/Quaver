using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartStateMachines
{
    [MoonSharpVisible(false)] public ElementAccessShortcut Shortcut { get; }

    public ModChartStateMachines(GameplayScreenView gameplayScreenView)
    {
        Shortcut = new ElementAccessShortcut(gameplayScreenView);
    }

    public readonly OrthogonalStateMachine RootMachine = new();

    public OrthogonalStateMachine NewOrthogonal(string name = "", StateMachineState parent = default) =>
        new(name, parent);

    public StateMachine.StateMachine NewMachine(string name = "", StateMachineState entryState = null,
        StateMachineState parent = default) => new(entryState, name, parent);

    public LuaStateMachineState NewState(string name = "", Closure updater = null, Closure onEnable = null,
        Closure onDisable = null, StateMachineState parent = default)
    {
        return new LuaStateMachineState(updater, onEnable, onDisable, name, parent);
    }

    /// <summary>
    ///     Attempts to change state from one to another
    ///     This is responsible for calling the respective chain of <see cref="StateMachineState.Leave"/> and <see cref="StateMachineState.Enter"/>,
    ///     along the path <see cref="originalState"/> ~> LCA ~> <see cref="targetState"/>. <br/>
    ///     Currently, this method has the time complexity of O(h), where h is the depth of <see cref="originalState"/> <br/>
    /// </summary>
    /// <param name="originalState"></param>
    /// <param name="targetState"></param>
    /// <exception cref="InvalidOperationException"><see cref="originalState"/> and <see cref="targetState"/> do not share an ancestor</exception>
    public void ChangeState(StateMachineState originalState, StateMachineState targetState)
    {
        // TODO: Use LCA -> RMQ for O(n) preprocessing and O(1) query
        if (StateMachineState.DisjointSetUnion.IsUnion(originalState, targetState))
        {
            throw new InvalidOperationException(
                $"Unable to transition from state '{originalState.Name}' to '{targetState.Name}' because they do not share an ancestor state machine");
        }

        if (originalState == targetState) return;

        var currentState = originalState;
        while (currentState != null)
        {
            currentState.LastLcaSearchTarget = originalState;
            if (currentState.Parent != null)
                currentState.Parent.LastLcaSearchChild = currentState;
            currentState = currentState.Parent;
        }

        // Find the LCA of two states (Least Common Ancestor)
        var lca = targetState;
        while (lca != null && lca.LastLcaSearchTarget != originalState)
        {
            lca = lca.Parent;
        }

        // Call a sequence of OnLeave()
        lca?.LastLcaSearchChild.Leave();
        targetState.Enter();
    }
}