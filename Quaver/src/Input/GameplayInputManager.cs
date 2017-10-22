using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Quaver.GameState;
using Quaver.Config;
using Quaver.Gameplay;

namespace Quaver.Input
{
    internal class GameplayInputManager : IInputManager
    {
        /// <summary>
        ///     The current State
        /// </summary>
        public State CurrentState { get; set; } = State.PlayScreen;

        /// <summary>
        ///     The current Keyboard State
        /// </summary>
        public KeyboardState KeyboardState { get; set; }

        /// <summary>
        ///     All of the lane keys mapped to a list
        /// </summary>
        private List<Keys> LaneKeys { get; } = new List<Keys>()
        {
            Configuration.KeyMania1,
            Configuration.KeyMania2,
            Configuration.KeyMania3,
            Configuration.KeyMania4
        };

        /// <summary>
        ///     Checks if the given input was given
        /// </summary>
        public void CheckInput()
        {
            // Set the current state of the keyboard.
            KeyboardState = Keyboard.GetState();

            // Update Lane Keys Receptor
            var updatedReceptor = false;
            for (var i = 0; i < LaneKeys.Count; i++)
                updatedReceptor = (KeyboardState.IsKeyDown(LaneKeys[i])) ? Playfield.UpdateReceptor(i, true) : Playfield.UpdateReceptor(i, false);              
        }
    }
}
