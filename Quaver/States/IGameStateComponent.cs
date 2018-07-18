namespace Quaver.States
{
    internal interface IGameStateComponent
    {
        /// <summary>
        ///     Initializes the componenet
        /// </summary>
        /// <param name="state"></param>
        void Initialize(IGameState state);

        /// <summary>
        ///     Unloads/Frees memory from this component on-exit.
        /// </summary>
        void UnloadContent();

        /// <summary>
        ///     Update, part of the game loop
        /// </summary>
        /// <param name="dt"></param>
        void Update(double dt);

        /// <summary>
        ///     Draws the game component
        /// </summary>
        void Draw();
    }
}
