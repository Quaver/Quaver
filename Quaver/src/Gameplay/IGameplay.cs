using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.GameState.States;

namespace Quaver.Gameplay
{
    internal abstract class IGameplay
    {

        internal PlayScreenState PlayScreen { get; set; }

        /// <summary>
        ///     Any initialization before the state begins.
        /// </summary>
        internal virtual void Initialize(PlayScreenState playScreen)
        {
            
        }

        /// <summary>
        ///     Any unloading of content.
        /// </summary>
        internal virtual void UnloadContent()
        {
            
        }

        /// <summary>
        ///     Called each frame. All game logic goes here.
        /// </summary>
        /// <param name="gameTime"></param>
        internal virtual void Update(double dt)
        {
            
        }

        /// <summary>
        ///     Handles drawing things to the screen.
        /// </summary>
        internal virtual void Draw()
        {
            
        }
    }
}
