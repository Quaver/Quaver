namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

public enum TransitionStatus
{
    Possible,
    Unreachable,
    Incompatible,
    Self,
    InactiveOrigin,
    ActiveTarget
}