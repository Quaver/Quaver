using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

[MoonSharpUserData]
public class LuaStateMachineState : StateMachineState
{
    private Closure OnUpdateClosure { get; }

    private Closure OnEnableClosure { get; }

    private Closure OnDisableClosure { get; }

    public LuaStateMachineState(ModChartScript script, Closure onUpdateClosure, Closure onEnableClosure, Closure onDisableClosure, 
        string name = "", StateMachineState parent = default) : base(script, name, parent)
    {
        OnUpdateClosure = onUpdateClosure;
        OnEnableClosure = onEnableClosure;
        OnDisableClosure = onDisableClosure;
    }

    public override void Update()
    {
        base.Update();
        OnUpdateClosure?.SafeCall();
    }

    public override void Enter()
    {
        base.Enter();
        OnEnableClosure?.SafeCall();
    }

    public override void Leave()
    {
        base.Leave();
        OnDisableClosure?.SafeCall();
    }
}