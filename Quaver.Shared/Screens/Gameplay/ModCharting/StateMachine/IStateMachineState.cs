namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

public interface IStateMachineState
{
    
    int Id { get; set; }
    int OnUpdate();
    void OnInitialize();
    void OnEnable();
    void OnDisable();
}