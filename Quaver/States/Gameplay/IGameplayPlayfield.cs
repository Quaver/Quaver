using Quaver.Graphics.Sprites;

namespace Quaver.States.Gameplay
{
    internal interface IGameplayPlayfield : IGameStateComponent
    {
        /// <summary>
        ///     The playfield.
        /// </summary>
        QuaverContainer Container { get; set; }
    }
}