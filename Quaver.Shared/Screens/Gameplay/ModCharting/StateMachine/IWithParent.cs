namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

public interface IWithParent<T> where T : IWithParent<T>
{
    T Parent { get; }
}