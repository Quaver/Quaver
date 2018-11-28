using Microsoft.Xna.Framework;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Gameplay.Rulesets
{
    public interface IGameplayPlayfield : IGameScreenComponent
    {
        /// <summary>
        ///     Container that has the entire playfield in it.
        /// </summary>
        Container Container { get; set; }

        /// <summary>
        ///     Handles what happens to the playfield on failure.
        /// </summary>
        /// <param name="gameTime"></param>
        void HandleFailure(GameTime gameTime);
    }
}
