using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

[MoonSharpUserData]
public class LuaStateMachineState : StateMachineState
{
    private Closure OnUpdateClosure { get; }

    private Closure OnEnableClosure { get; }

    private Closure OnDisableClosure { get; }

    public LuaStateMachineState(Closure onUpdateClosure, Closure onEnableClosure, Closure onDisableClosure,
        string name = "", StateMachineState parent = default) : base(name, parent)
    {
        OnUpdateClosure = onUpdateClosure;
        OnEnableClosure = onEnableClosure;
        OnDisableClosure = onDisableClosure;
    }

    public override void Update()
    {
        base.Update();
        OnUpdateClosure.SafeCall();
    }

    public override void Enter()
    {
        base.Enter();
        OnEnableClosure.SafeCall();
    }

    public override void Leave()
    {
        base.Leave();
        OnDisableClosure.SafeCall();
    }
}