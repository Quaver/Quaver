using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

public class LuaStateMachineState : StateMachineState
{
    private Closure OnUpdateClosure { get; }

    private Closure OnEnableClosure { get; }

    private Closure OnDisableClosure { get; }

    public LuaStateMachineState(Closure updater, Closure onEnable, Closure onDisable)
    {
        OnUpdateClosure = updater;
        OnEnableClosure = onEnable;
        OnDisableClosure = onDisable;
    }

    public override void OnUpdate()
    {
        OnUpdateClosure.Call();
    }

    public override void OnEnter()
    {
        OnEnableClosure.Call();
    }

    public override void OnLeave()
    {
        OnDisableClosure.Call();
    }
}