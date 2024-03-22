using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.StateMachine;

public interface IStateMachineState
{
    
    int Id { get; set; }
    int OnUpdate();
    void OnInitialize();
    void OnEnable();
    void OnDisable();
}