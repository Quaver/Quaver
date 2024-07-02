using System;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

[MoonSharpUserData]
public class CustomStateMachineState : StateMachineState
{
    private Action<CustomStateMachineState> OnUpdateClosure { get; }

    private Action<CustomStateMachineState> OnEnableClosure { get; }

    private Action<CustomStateMachineState> OnDisableClosure { get; }

    public CustomStateMachineState(ModChartScript script, Action<CustomStateMachineState> onUpdateClosure,
        Action<CustomStateMachineState> onEnableClosure, Action<CustomStateMachineState> onDisableClosure,
        string name = "", StateMachineState parent = default) : base(script, name, parent)
    {
        OnUpdateClosure = onUpdateClosure;
        OnEnableClosure = onEnableClosure;
        OnDisableClosure = onDisableClosure;
    }

    public override void Update()
    {
        base.Update();
        OnUpdateClosure?.Invoke(this);
    }

    public override void Enter()
    {
        base.Enter();
        OnEnableClosure?.Invoke(this);
    }

    public override void Leave()
    {
        base.Leave();
        OnDisableClosure?.Invoke(this);
    }
}