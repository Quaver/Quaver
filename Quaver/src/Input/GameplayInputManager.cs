using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Audio;
using Quaver.GameState;
using Quaver.Config;
using Quaver.Database;
using Quaver.Gameplay;
using Quaver.GameState.States;
using Quaver.Logging;
using Quaver.QuaFile;

namespace Quaver.Input
{
    internal class GameplayInputManager : IInputManager
    {
        /// <summary>
        ///     The current State
        /// </summary>
        public State CurrentState { get; set; } = State.PlayScreen;

        /// <summary>
        ///     The PlayScreenState this Input Manager is referencing
        /// </summary>
        private NoteManager NoteManager { get; set; }

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
        ///     Keeps track of unique key presses
        /// </summary>
        private bool[] LaneKeyDown { get; set; } = new bool[4];

        /// <summary>
        ///     Keeps track of whether or not the song intro was skipped.
        /// </summary>
        private bool IntroSkipped { get; set; }

        internal GameplayInputManager(NoteManager noteManager)
        {
            NoteManager = noteManager;
        }

        /// <summary>
        ///     Checks if the given input was given
        /// </summary>
        public void CheckInput(Qua qua, bool skippable)
        {
            // Check Mania Key Presses
            HandleManiaKeyPresses();

            // Check Skip Song Input
            SkipSong(qua, skippable);

            // Check import beatmaps
            ImportBeatmaps();

            // Pause game
            HandlePause();
        }

        /// <summary>
        ///     Handles what happens when a mania key is pressed and released.
        /// </summary>
        private void HandleManiaKeyPresses()
        {
            // Update Lane Keys Receptor
            for (var i = 0; i < LaneKeys.Count; i++)
            {
                //Lane Key Press
                if (GameBase.KeyboardState.IsKeyDown(LaneKeys[i]) && !LaneKeyDown[i])
                {
                    LaneKeyDown[i] = true;
                    NoteManager.Input(i,true);
                    GameBase.LoadedSkin.Hit.Play((float)Configuration.VolumeGlobal / 100 * Configuration.VolumeEffect / 100, 0, 0);
                }
                //Lane Key Release
                else if (GameBase.KeyboardState.IsKeyUp(LaneKeys[i]) && LaneKeyDown[i])
                {
                    LaneKeyDown[i] = false;
                    NoteManager.Input(i, false);
                }
            }
        }

        /// <summary>
        ///     Detects if the song intro is skippable and will skip if the player decides to.
        /// </summary>
        /// <param name="qua"></param>
        /// <param name="currentSongTime"></param>
        private void SkipSong(Qua qua, bool skippable)
        {
            if (skippable && GameBase.KeyboardState.IsKeyDown(Configuration.KeySkipIntro) && !IntroSkipped)
            {
                IntroSkipped = true;

                Logger.Log("Song has been successfully skipped to 3 seconds before the first HitObject.", Color.Pink);

                // Skip to 3 seconds before the notes start
                SongManager.SkipTo(qua.HitObjects[0].StartTime - 3000);
                SongManager.Play();
                NoteManager.PlayScreen.Timing.SongIsPlaying = true;

                GameBase.ChangeDiscordPresenceGameplay(true);
            }
        }

        /// <summary>
        ///     Checks if the beatmap import queue is ready, and imports then if the user decides to.
        /// </summary>
        private void ImportBeatmaps()
        {
            // TODO: This is a beatmap import and sync test, eventually add this to its own game state
            if (GameBase.KeyboardState.IsKeyDown(Keys.F5) && GameBase.ImportQueueReady)
            {
                GameBase.ImportQueueReady = false;

                // Asynchronously load and set the GameBase beatmaps and visible ones.
                Task.Run(async () =>
                {
                    await GameBase.LoadAndSetBeatmaps();
                    GameBase.VisibleBeatmaps = GameBase.Beatmaps;
                });
            }
        }

        /// <summary>
        ///     Responsible for handling pausing 
        /// </summary>
        private void HandlePause()
        {
            if (!GameBase.KeyboardState.IsKeyDown(Configuration.KeyPause))
                return;

            // TODO: Implement actual pausing here. For now, we're just going to go back to the main menu.
            SongManager.Pause();
            GameBase.GameStateManager.ChangeState(new MainMenuState());
        }
    }
}
