namespace Quaver.Shared.Screens.Gameplay
{
    public interface IGameplayInputManager
    {
        /// <summary>
        ///     Handles all of the input for the entire ruleset.
        /// </summary>
        /// <param name="dt"></param>
        void HandleInput(double dt);
    }
}
