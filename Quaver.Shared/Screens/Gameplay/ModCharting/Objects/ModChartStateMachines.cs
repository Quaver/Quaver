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

    public ModChartStateMachine New()
    {
        var machine = new ModChartStateMachine();
        Shortcut.GameplayScreenView.StoryboardStateMachines.Add(machine);
        return machine;
    }

    public void Delete(ModChartStateMachine machine)
    {
        Shortcut.GameplayScreenView.StoryboardStateMachines.Remove(machine);
    }

    public LuaStateMachineState NewState(Closure updater, Closure onEnable,
        Closure onDisable)
    {
        return new LuaStateMachineState(updater, onEnable, onDisable);
    }

    public void ChangeState(StateMachineState originalState, StateMachineState targetState)
    {
        if (StateMachineState.DisjointSetUnion.GetRepresentative(originalState) !=
            StateMachineState.DisjointSetUnion.GetRepresentative(targetState))
        {
            throw new InvalidOperationException(
                $"Unable to transition from state '{originalState.Name}' to '{targetState.Name}' because they do not share an ancestor state machine");
        }

        var currentState = originalState.Parent;
        while (currentState != null)
        {
            currentState.LastLcaSearchTarget = originalState;
            currentState = currentState.Parent;
        }

        // Find the LCA of two states (Least Common Ancestor)
        var lca = targetState;
        // Maintain a stack to call a sequence of OnEntry()
        var enterSequence = new Stack<StateMachineState>();
        while (lca != null && lca.LastLcaSearchTarget != originalState)
        {
            enterSequence.Push(lca);
            lca = lca.Parent;
        }

        // Call a sequence of OnLeave()
        currentState = originalState;
        while (currentState != lca)
        {
            currentState.OnLeave();
            currentState = currentState.Parent;
        }

        // Call the sequence of OnEnter()
        while (enterSequence.TryPop(out var state))
        {
            state.OnEnter();
        }
    }
}