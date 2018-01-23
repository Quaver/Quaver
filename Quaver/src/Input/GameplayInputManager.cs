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
using Quaver.GameState.States;
using Quaver.Logging;
using Quaver.Replays;
using Quaver.GameState.Gameplay;

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
        ///     A reference of all of the lane keys mapped to a list - 4K
        /// </summary>
        private List<Keys> LaneKeys { get; } = new List<Keys>()
        {
            Configuration.KeyMania4k1,
            Configuration.KeyMania4k2,
            Configuration.KeyMania4k3,
            Configuration.KeyMania4k4
        };

        /// <summary>
        ///     a reference of all of the lane keys mapped to a list - 7K
        /// </summary>
        private List<Keys> LaneKeys7K { get; } = new List<Keys>()
        {
            Configuration.KeyMania7k1,
            Configuration.KeyMania7k2,
            Configuration.KeyMania7k3,
            Configuration.KeyMania7k4,
            Configuration.KeyMania7k5,
            Configuration.KeyMania7k6,
            Configuration.KeyMania7k7
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
        public void CheckInput(bool skippable, List<ReplayFrame> ReplayFrames)
        {
            // Pause game
            //HandlePause();

            // Check Mania Key Presses
            HandleManiaKeyPresses();

            // Check skip
            if (SkipKeyDown && GameBase.KeyboardState.IsKeyUp(Configuration.KeySkipIntro))
            {
                SkipKeyDown = false;
            }
            else if (!SkipKeyDown && GameBase.KeyboardState.IsKeyDown(Configuration.KeySkipIntro))
            {
                SkipKeyDown = true;
                SkipSong?.Invoke(this, null);
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
                    //NoteManager.Input(i,true);
                    //GameBase.LoadedSkin.Hit.Play((float)Configuration.VolumeGlobal / 100 * Configuration.VolumeEffect / 100, 0, 0);
                }
                //Lane Key Release
                else if (GameBase.KeyboardState.IsKeyUp(InputManiaKeys[i]) && LaneKeyDown[i])
                {
                    LaneKeyDown[i] = false;
                    ManiaKeyRelease?.Invoke(this, new ManiaKeyEventArgs(i));
                    //NoteManager.Input(i, false);
                }
            }
        }
    }
}
