using Quaver.API.Enums;
using Quaver.Graphics.Sprites;

namespace Quaver.States.Gameplay
{
    internal abstract class GameModeRuleset
    {
        /// <summary>
        ///     The game mode this playfield is for.
        /// </summary>
        internal GameMode Mode { get; set; }

        /// <summary>
        ///     The playfield that this game mode will be drawn to.
        /// </summary>
        internal QuaverContainer Playfield { get; set; }

        /// <summary>
        ///     Initializes the game mode ruleset.
        /// </summary>
        internal virtual void Initialize()
        {
            Playfield = new QuaverContainer();
            CreatePlayfield();
        }

         /// <summary>
        ///     Updates the game mode ruleset.
        /// </summary>
        /// <param name="dt"></param>
        internal virtual void Update(double dt)
        {
            Playfield.Update(dt);    
            HandleInput(dt);
        }

        /// <summary>
        ///     Draws the game mode.
        /// </summary>
        internal virtual void Draw()
        {
            Playfield.Draw();
        }

        /// <summary>
        ///     Destroys the game mode.
        /// </summary>
        internal virtual void Destroy()
        {
            Playfield.Destroy();
        }
        

        /// <summary>
        ///     Handles the input of the game mode.
        /// </summary>
        /// <param name="dt"></param>
        protected abstract void HandleInput(double dt);

        /// <summary>
        ///     Creates the actual playfield.
        /// </summary>
        protected abstract void CreatePlayfield();
    }
}