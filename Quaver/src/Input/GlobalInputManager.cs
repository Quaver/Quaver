using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;
using Microsoft.Xna.Framework.Input;
using Quaver.Audio;
using Quaver.Commands;
using Quaver.GameState;
using Quaver.Main;

namespace Quaver.Input
{
    internal class GlobalInputManager : IInputManager
    {
        /// <summary>
        ///     The current state for the specifc input manager
        /// </summary>
        public State CurrentState { get; set; } // Global State, so this isn't necessary.

        /// <summary>
        ///     Keeps track of the last scroll wheel value.
        /// </summary>
        private int LastScrollWheelValue { get; set; }

        /// <summary>
        ///     Check the input.
        /// </summary>
        public void CheckInput()
        {
            HandleVolumeChanges();
        }

        /// <summary>
        ///     Handles all global volume changes.
        ///     For this to be activated, the user must be holding down either ALT key while they are scrolling the mouse.
        /// </summary>
        private void HandleVolumeChanges()
        {
            //  Raise volume if the user scrolls up.
            if (GameBase.MouseState.ScrollWheelValue > LastScrollWheelValue 
                && (GameBase.KeyboardState.IsKeyDown(Keys.RightAlt) || GameBase.KeyboardState.IsKeyDown(Keys.LeftAlt)) 
                && Config.Configuration.VolumeGlobal < 100)
            {
                Config.Configuration.VolumeGlobal += 5;

                // Set the last scroll wheel value
                LastScrollWheelValue = GameBase.MouseState.ScrollWheelValue;

                // Change the master volume based on the new config value.
                AudioHandler.ChangeMasterVolume();
                Console.WriteLine($"[CONFIG MANAGER] VolumeGlobal Changed To: {Config.Configuration.VolumeGlobal}");
            }
            // Lower volume if the user scrolls down
            else if (GameBase.MouseState.ScrollWheelValue < LastScrollWheelValue 
                && (GameBase.KeyboardState.IsKeyDown(Keys.RightAlt) || GameBase.KeyboardState.IsKeyDown(Keys.LeftAlt)) 
                && Config.Configuration.VolumeGlobal > 0)
            {
                Config.Configuration.VolumeGlobal -= 5;

                // Set the last scroll wheel value
                LastScrollWheelValue = GameBase.MouseState.ScrollWheelValue;

                // Change the master volume based on the new config value.
                AudioHandler.ChangeMasterVolume();
                Console.WriteLine($"[CONFIG MANAGER] VolumeGlobal Changed To: {Config.Configuration.VolumeGlobal}");
            }
        }
    }
}
