using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.StateMachine;

public class StoryboardStateMachineState
{
    public delegate int UpdateDelegate();

    public delegate void OnInitializeDelegate();
    
    public delegate void OnEnableDelegate();

    public delegate void OnDisableDelegate();

    public int Id { get; set; }

    public UpdateDelegate Update { get; set; }

    public OnInitializeDelegate OnInitialize { get; set; }
    
    public OnEnableDelegate OnEnable { get; set; }
    
    public OnDisableDelegate OnDisable { get; set; }

    public StoryboardStateMachineState(OnInitializeDelegate onInitialize, UpdateDelegate update,  OnEnableDelegate onEnable, OnDisableDelegate onDisable)
    {
        Id = -2;
        Update = update;
        OnInitialize = onInitialize;
        OnEnable = onEnable;
        OnDisable = onDisable;
    }

    public StoryboardStateMachineState(Closure onInitialize, Closure updater, Closure onEnable, Closure onDisable)
    {
        Id = -2;
        Update = () => updater.Call().ToObject<int>();
        OnInitialize = () => onInitialize.Call();
        OnEnable = () => onEnable.Call();
        OnDisable = () => onDisable.Call();
    }
}