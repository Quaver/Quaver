using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Quaver.GameState
{
    internal interface IGameState
    {
        /// <summary>
        ///     The current state as defined in the enum.
        /// </summary>
        State CurrentState { get; set; }

        /// <summary>
        ///     Once the state is initialized, this value determines if the state can be updated.
        /// </summary>
        bool UpdateReady { get; set; }

        /// <summary>
        ///     Any initialization before the state begins.
        /// </summary>
        void Initialize();

        /// <summary>
        ///     Any unloading of content.
        /// </summary>
        void UnloadContent();

        /// <summary>
        ///     Called each frame. All game logic goes here.
        /// </summary>
        /// <param name="gameTime"></param>
        void Update(double dt);

        /// <summary>
        ///     Handles drawing things to the screen.
        /// </summary>
        void Draw();
    }
}
