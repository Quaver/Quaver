using Microsoft.Xna.Framework;

namespace Quaver.Shared.Screens
{
    public interface IGameScreenComponent
    {
        /// <summary>
        ///     Update, part of the game loop
        /// </summary>
        /// <param name="gameTime"></param>
        void Update(GameTime gameTime);

        /// <summary>
        ///     Draws the game component
        /// </summary>
        void Draw(GameTime gameTime);

        /// <summary>
        ///     Unloads/Frees memory from this component
        /// </summary>
        void Destroy();
    }
}
