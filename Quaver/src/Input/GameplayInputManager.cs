﻿using System;
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
using Quaver.Enums;
using Quaver.GameState.States;
using Quaver.Logging;
using Quaver.QuaFile;
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
        ///     Is the game currently paused?
        /// </summary>
        private bool IsPaused { get; set; }

        /// <summary>
        ///     Keeps track of if the pause key is actually down.
        /// </summary>
        private bool PauseKeyDown { get; set; }

        /// <summary>
        ///     A reference of all of the lane keys mapped to a list - 4K
        /// </summary>
        private List<Keys> LaneKeys { get; } = new List<Keys>()
        {
            Configuration.KeyMania1,
            Configuration.KeyMania2,
            Configuration.KeyMania3,
            Configuration.KeyMania4
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
        public event EventHandler<ManiaKey> ManiaKeyPress;

        /// <summary>
        ///     EventHandler for when ever a mania key gets released
        /// </summary>
        public event EventHandler<ManiaKey> ManiaKeyRelease;

        /// <summary>
        ///     Keeps track of whether or not the song intro was skipped.
        /// </summary>
        private bool IntroSkipped { get; set; }

        /// <summary>
        ///     Checks if the given input was given
        /// </summary>
        public void CheckInput(bool skippable, List<ReplayFrame> ReplayFrames)
        {
            // Pause game
            //HandlePause();

            // Don't handle the below if the game is paused.
            if (IsPaused)
                return;

            // Check Mania Key Presses
            HandleManiaKeyPresses();

            // Check Skip Song Input
            //SkipSong(skippable);

            // Add replay frames
            ReplayHelper.AddReplayFrames(ReplayFrames, GameBase.SelectedBeatmap.Qua);
        }

        /// <summary>
        ///     Handles what happens when a mania key is pressed and released.
        /// </summary>
        private void HandleManiaKeyPresses()
        {
            var inputKeys = new List<Keys>();

            // Determine which set of keys to use based on the .qua
            switch (GameBase.SelectedBeatmap.Qua.Mode)
            {
                case GameModes.Keys4:
                    inputKeys = LaneKeys;
                    break;
                case GameModes.Keys7:
                    inputKeys = LaneKeys7K;
                    break;
                default:
                    break;
            }

            // Update Lane Keys Receptor
            for (var i = 0; i < inputKeys.Count; i++)
            {
                //Lane Key Press
                if (GameBase.KeyboardState.IsKeyDown(inputKeys[i]) && !LaneKeyDown[i])
                {
                    LaneKeyDown[i] = true;
                    ManiaKeyPress?.Invoke(this, new ManiaKey(i));
                    //NoteManager.Input(i,true);
                    //GameBase.LoadedSkin.Hit.Play((float)Configuration.VolumeGlobal / 100 * Configuration.VolumeEffect / 100, 0, 0);
                }
                //Lane Key Release
                else if (GameBase.KeyboardState.IsKeyUp(inputKeys[i]) && LaneKeyDown[i])
                {
                    LaneKeyDown[i] = false;
                    ManiaKeyRelease?.Invoke(this, new ManiaKey(i));
                    //NoteManager.Input(i, false);
                }
            }
        }

        /*
        /// <summary>
        ///     Detects if the song intro is skippable and will skip if the player decides to.
        /// </summary>
        /// <param name="qua"></param>
        /// <param name="currentSongTime"></param>
        private void SkipSong(bool skippable)
        {
            if (skippable && GameBase.KeyboardState.IsKeyDown(Configuration.KeySkipIntro) && !IntroSkipped)
            {
                IntroSkipped = true;

                Logger.Log("Song has been successfully skipped to 3 seconds before the first HitObject.", Color.Pink);

                // Skip to 3 seconds before the notes start
                SongManager.Load();
                SongManager.SkipTo(GameBase.SelectedBeatmap.Qua.HitObjects[0].StartTime - 3000 + SongManager.BassDelayOffset);
                SongManager.Play();

                //NoteManager.PlayScreen.Timing.SongIsPlaying = true;

                GameBase.ChangeDiscordPresenceGameplay(true);
            }
        }

        /// <summary>
        ///     Responsible for handling pausing 
        /// </summary>
        private void HandlePause()
        {
            // TODO: Fix this and add pausing here - Before the song begins.
            if (SongManager.Position == 0)
                return;

            if (GameBase.KeyboardState.IsKeyUp(Configuration.KeyPause))
                PauseKeyDown = false;

            // Prevent holding the pause key down
            if (PauseKeyDown || !GameBase.KeyboardState.IsKeyDown(Configuration.KeyPause))
                return;

            PauseKeyDown = true;

            // TODO: Implement actual pausing here. For now, we're just going to go back to the main menu.
            IsPaused = !IsPaused;

            if (IsPaused)
            {
                SongManager.Pause();
                
                // Change Rich Presence
                GameBase.ChangeDiscordPresence($"{GameBase.SelectedBeatmap.Qua.Artist} - {GameBase.SelectedBeatmap.Qua.Title} [{GameBase.SelectedBeatmap.Qua.DifficultyName}]", "Paused");
                return;
            }

            SongManager.Resume();
            GameBase.ChangeDiscordPresenceGameplay(true);
        }*/
    }
}
