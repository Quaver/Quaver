using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.StateMachine;

public class LuaStateMachineState : IStateMachineState
{
    private Closure OnUpdateClosure { get; }

    private Closure OnInitializeClosure { get; }

    private Closure OnEnableClosure { get; }

    private Closure OnDisableClosure { get; }

    public LuaStateMachineState(Closure onInitialize, Closure updater, Closure onEnable, Closure onDisable)
    {
        Id = -2;
        OnInitializeClosure = onInitialize;
        OnUpdateClosure = updater;
        OnEnableClosure = onEnable;
        OnDisableClosure = onDisable;
    }

    public int Id { get; set; }

    public int OnUpdate()
    {
        return OnUpdateClosure.Call().ToObject<int>();
    }

    public void OnInitialize()
    {
        OnInitializeClosure.Call();
    }

    public void OnEnable()
    {
        OnEnableClosure.Call();
    }

    public void OnDisable()
    {
        OnDisableClosure.Call();
    }
}