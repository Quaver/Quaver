using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Beatmaps;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Input;
using Quaver.Main;
using Quaver.QuaFile;
using Quaver.Utility;

namespace Quaver.Gameplay
{
    /// <summary>
    ///     This is the GameState when the player is actively playing.
    /// </summary>
    internal partial class StatePlayScreen : IGameState
    {
        public State CurrentState { get; set; } = State.PlayScreen;

        /// <summary>
        ///     The input manager for this game state.
        /// </summary>
        private GameplayInputManager InputManager { get; } = new GameplayInputManager();

        /// <summary>
        ///     The Qua object - Parsed .qua file.
        /// </summary>
        private Qua Qua { get; set; }

        /// <summary>
        ///     The scroll speed
        /// </summary>
        private float ScrollSpeed { get; set; } = Configuration.ScrollSpeed / 20f;

        /// <summary>
        ///     TODO: Add Summary.
        /// </summary>
        private float ScrollNegativeFactor { get; set; } = 1f;

        /// <summary>
        ///     Test mod for No Slider Velcoities
        /// </summary>
        private bool ModNoSv { get; }

        /// <summary>
        ///     Other random mods that were put here.
        /// </summary>
        private bool ModPull { get; }
        private bool ModSplit { get; }
        private bool ModSpin { get; }
        private bool ModShuffle { get; }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Initialize()
        {
            // TODO: MOVE THE LOADING OF THE AUDIO, PARSING OF BEATMAPS, AND LOADING OF HIT OBJECTS TO A LOADING STATE. VERY IMPORTANT
            // Load up the selected beatmap's song, yo.
            GameBase.SelectedBeatmap.Song = new GameAudio(GameBase.SelectedBeatmap.AudioPath);

            // Parse the selected beatmap.
            Qua = new Qua(GameBase.SelectedBeatmap.Path);

            Console.WriteLine("[STATE_PLAYSCREEN]: Initialized Gameplay State.");
            Console.WriteLine("Loaded Beatmap: {0} - {1}", GameBase.SelectedBeatmap.Artist, GameBase.SelectedBeatmap.Title);

            //Create loggers
            LogTracker.AddLogger("DeltaTime", Color.LawnGreen);
            LogTracker.AddLogger("SongTime", Color.White);
            LogTracker.AddLogger("SongPos", Color.White);
            LogTracker.AddLogger("HitObjects", Color.Wheat);
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void LoadContent()
        {
            //Initialize Components
            Playfield.InitializePlayfield();
            InitializeTiming();
            InitializeNotes();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void UnloadContent()
        {
            GameStateManager.Instance.UnloadContent();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Get the current game time in milliseconds.
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Set the current song time.
            SetCurrentSongTime(dt);

            // Update the playfield
            Playfield.UpdatePlayfield(dt); ;

            // Update the Notes
            UpdateNotes(dt);

            // Check the input for this particular game state.
            InputManager.CheckInput();

            // Update Loggers
            LogTracker.UpdateLogger("DeltaTime", "Delta Time: " + dt + "ms");
            LogTracker.UpdateLogger("SongTime", "Current Song Time: " + _currentSongTime + "ms");
            LogTracker.UpdateLogger("SongPos", "Current Track Position: " + _trackPosition);
            LogTracker.UpdateLogger("HitObjects", "Total Remaining Notes: " + _hitObjectQueue.Count);
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Draw()
        {
            //Playfield.PlayfieldBoundary.Draw();
        }
    }
}