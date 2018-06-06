using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;

namespace Quaver.States.Gameplay
{
    internal interface IGameplayPlayfield : IGameStateComponent
    {
        /// <summary>
        ///     The playfield.
        /// </summary>
        Container Container { get; set; }

        /// <summary>
        ///     Handles what happens to the playfield upon failure.
        /// </summary>
        /// <param name="dt"></param>
        void HandleFailure(double dt);
    }
}