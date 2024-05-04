using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartStateMachines
{
    [MoonSharpVisible(false)] public ElementAccessShortcut Shortcut { get; }

    public ModChartStateMachines(ElementAccessShortcut shortcut)
    {
        Shortcut = shortcut;
        RootMachine = new OrthogonalStateMachine(Shortcut.ModChartScript, "Root");
    }

    public OrthogonalStateMachine RootMachine { get; }

    public OrthogonalStateMachine NewOrthogonal(string name = "", StateMachineState parent = default) =>
        new(Shortcut.ModChartScript, name, parent);

    public StateMachine.StateMachine NewMachine(string name = "", StateMachineState entryState = null,
        StateMachineState parent = default) => new(Shortcut.ModChartScript, entryState, name, parent);

    public LuaStateMachineState NewState(string name = "", Closure updater = null, Closure onEnable = null,
        Closure onDisable = null, StateMachineState parent = default)
    {
        return new LuaStateMachineState(Shortcut.ModChartScript, updater, onEnable, onDisable, name, parent);
    }

    public void Start()
    {
        RootMachine.Enter();
    }

    public void Stop()
    {
        RootMachine.Leave();
    }

    /// <summary>
    ///     Tries to find the LCA (Least Common Ancestor) of the original and target state.
    ///     Returns the viability of the transition
    /// </summary>
    /// <remarks>
    ///     Transition path: s ~> u ~> lca ~> v ~> t <br/>
    ///     Let P1 = s ~> u, P2 = v ~> t <br/>
    ///     s, t are source and target, respectively <br/>
    ///     u, v are direct children of lca <br/>
    ///     u != v, s != t, P1 INTERSECT P2 = {} <br/>
    ///     it is possible that s = u or v = t <br/>
    ///     it is also possible that s = lca or t = lca, in which case u, v = null <br/>
    ///     All states in the path P2 except v should be inactive, and have to be an entry state <br/>
    ///     We exclude v because we consider a simple transition x -> y where x.parent = y.parent <br/>
    /// </remarks>
    /// <param name="originalState"></param>
    /// <param name="targetState"></param>
    /// <param name="lca">LCA found, if possible, otherwise null</param>
    /// <returns></returns>
    public static TransitionStatus FindLca(StateMachineState originalState, StateMachineState targetState,
        out StateMachineState lca)
    {
        lca = null;
        // TODO: Use LCA -> RMQ for O(n) preprocessing and O(1) query
        if (!StateMachineState.DisjointSetUnion.IsUnion(originalState, targetState))
        {
            return TransitionStatus.Unreachable;
        }

        // Self transition: skip
        if (originalState == targetState) return TransitionStatus.Self;
        // Inappropriate transition: the original state must be active
        if (!originalState.IsActive) return TransitionStatus.InactiveOrigin;
        if (targetState.IsActive) return TransitionStatus.ActiveTarget;

        // We will calculate LCA as well as u and v
        var currentState = originalState;
        currentState.LastLcaSearchChildToSource = null; // Nothing will leave if lca == originalState
        while (currentState != null)
        {
            currentState.LastLcaSearchTarget = originalState;
            if (currentState.Parent != null)
                currentState.Parent.LastLcaSearchChildToSource = currentState;
            currentState = currentState.Parent;
        }

        // Find the LCA of two states (Least Common Ancestor)
        lca = targetState;
        lca.LastLcaSearchChildToTarget = null;
        while (lca != null && lca.LastLcaSearchTarget != originalState)
        {
            if (lca.Parent != null)
                lca.Parent.LastLcaSearchChildToTarget = lca;
            lca = lca.Parent;
        }

        // Won't happen if DSU works well, but just in case
        if (lca == null) return TransitionStatus.Unreachable;

        currentState = targetState;
        // Logger.Debug($"{originalState} ~> {lca.LastLcaSearchChildToSource} ~> {lca} ~> {lca.LastLcaSearchChildToTarget} ~> {targetState}", LogType.Runtime);
        while (currentState != null && currentState != lca.LastLcaSearchChildToTarget)
        {
            if (currentState.IsActive) return TransitionStatus.ActiveTarget;
            if (currentState.Parent != null && !currentState.Parent.CanEnterSubStateDirectly(currentState))
                return TransitionStatus.Incompatible;
            currentState = currentState.Parent;
        }

        return TransitionStatus.Possible;
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
    public static void ChangeState(StateMachineState originalState, StateMachineState targetState)
    {
        var status = FindLca(originalState, targetState, out var lca);
        if (status != TransitionStatus.Possible)
            throw new InvalidOperationException($"Cannot transition from {originalState} to {targetState}: {status}");
        // Call a sequence of OnLeave()
        lca?.LastLcaSearchChildToSource.Leave();
        targetState.Enter();
    }
}