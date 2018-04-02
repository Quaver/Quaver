using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Audio;
using Quaver.GameState;
using Quaver.Config;
using Quaver.Database;
using Quaver.Logging;
using Quaver.Replays;
using Quaver.States;
using Quaver.States.Enums;

namespace Quaver.Input
{
    internal class GameplayInputManager : IInputManager
    {
        /// <summary>
        ///     The current State
        /// </summary>
        public State CurrentState { get; set; } = State.PlayScreen;

        /// <summary>
        ///     Keeps track of if the pause key is down.
        /// </summary>
        private bool PauseKeyDown { get; set; }

        /// <summary>
        ///     Keeps track if the skip key is down
        /// </summary>
        private bool SkipKeyDown { get; set; }

        /// <summary>
        ///     Keeps track if the pause button is down
        /// </summary>
        private bool PauseButtonDown { get; set; }

        /// <summary>
        ///     A reference of all of the lane keys mapped to a list - 4K
        /// </summary>
        private List<Keys> LaneKeys { get; } = new List<Keys>()
        {
            ConfigManager.KeyMania4k1,
            ConfigManager.KeyMania4k2,
            ConfigManager.KeyMania4k3,
            ConfigManager.KeyMania4k4
        };

        /// <summary>
        ///     a reference of all of the lane keys mapped to a list - 7K
        /// </summary>
        private List<Keys> LaneKeys7K { get; } = new List<Keys>()
        {
            ConfigManager.KeyMania7k1,
            ConfigManager.KeyMania7k2,
            ConfigManager.KeyMania7k3,
            ConfigManager.KeyMania7k4,
            ConfigManager.KeyMania7k5,
            ConfigManager.KeyMania7k6,
            ConfigManager.KeyMania7k7
        };

        /// <summary>
        ///     Keeps track of unique key presses - Initialized to 7 false bools, because the same is 
        ///     used for both 4k and 7k.
        /// </summary>
        public List<bool> LaneKeyDown { get; set; } = new List<bool>() { false, false, false, false, false, false, false };

        /// <summary>
        ///     EventHandler for when ever a mania key gets pressed
        /// </summary>
        public event EventHandler<ManiaKeyEventArgs> ManiaKeyPress;

        /// <summary>
        ///     EventHandler for when ever a mania key gets released
        /// </summary>
        public event EventHandler<ManiaKeyEventArgs> ManiaKeyRelease;

        /// <summary>
        ///     EventHandler for when ever the skip key gets pressed
        /// </summary>
        public event EventHandler SkipSong;

        /// <summary>
        ///     Event gets triggered everytime the player hits the pause key
        /// </summary>
        public event EventHandler PauseSong;

        /// <summary>
        ///     Mania keys for input
        /// </summary>
        private List<Keys> InputManiaKeys { get; set; }

        /// <summary>
        ///     Constructor
        /// </summary>
        public GameplayInputManager()
        {
            InputManiaKeys = new List<Keys>();
            // Determine which set of keys to use based on the .qua
            switch (GameBase.SelectedBeatmap.Qua.Mode)
            {
                case GameModes.Keys4:
                    InputManiaKeys = LaneKeys;
                    break;
                case GameModes.Keys7:
                    InputManiaKeys = LaneKeys7K;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     Checks if the given input was given
        /// </summary>
        public void CheckInput(bool skippable)
        {
            // Check Mania Key Presses
            HandleManiaKeyPresses();

            // Check skip
            if (SkipKeyDown && GameBase.KeyboardState.IsKeyUp(ConfigManager.KeySkipIntro))
            {
                SkipKeyDown = false;
            }
            else if (!SkipKeyDown && GameBase.KeyboardState.IsKeyDown(ConfigManager.KeySkipIntro))
            {
                SkipKeyDown = true;
                SkipSong?.Invoke(this, null);
            }

            // Check pause
            if (PauseButtonDown && GameBase.KeyboardState.IsKeyUp(ConfigManager.KeyPause))
            {
                PauseButtonDown = false;
            }
            else if (!PauseButtonDown && GameBase.KeyboardState.IsKeyDown(ConfigManager.KeyPause))
            {
                PauseButtonDown = true;
                PauseSong?.Invoke(this, null);
            }
        }

        /// <summary>
        ///     Handles what happens when a mania key is pressed and released.
        /// </summary>
        private void HandleManiaKeyPresses()
        {
            // Update Lane Keys Receptor
            for (var i = 0; i < InputManiaKeys.Count; i++)
            {
                //Lane Key Press
                if (GameBase.KeyboardState.IsKeyDown(InputManiaKeys[i]) && !LaneKeyDown[i])
                {
                    LaneKeyDown[i] = true;
                    ManiaKeyPress?.Invoke(this, new ManiaKeyEventArgs(i));
                }
                //Lane Key Release
                else if (GameBase.KeyboardState.IsKeyUp(InputManiaKeys[i]) && LaneKeyDown[i])
                {
                    LaneKeyDown[i] = false;
                    ManiaKeyRelease?.Invoke(this, new ManiaKeyEventArgs(i));
                }
            }
        }
    }
}
