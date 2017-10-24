using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Beatmaps;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Input;
using Quaver.Logging;
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
        /// <summary>
        ///     The current state as defined in the enum.
        /// </summary>
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
        ///     Keeps track of whether or not the song is current skippable.
        /// </summary>
        private bool SongSkippable { get; set; }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Initialize()
        {
            // TODO: MOVE THE LOADING OF THE AUDIO, PARSING OF BEATMAPS, AND LOADING OF HIT OBJECTS TO A LOADING STATE. VERY IMPORTANT
            // Load up the selected beatmap's song, yo.
            GameBase.SelectedBeatmap.LoadAudio();

            // Parse the selected beatmap.
            Qua = new Qua(GameBase.SelectedBeatmap.Path);

            Console.WriteLine($"[GAMEPLAY STATE] Initialized Gameplay State with Mods: { String.Join(", ", GameBase.CurrentGameModifiers.Select(x => x.ModIdentifier)) }");
            Console.WriteLine($"Loaded Beatmap: {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title} [{GameBase.SelectedBeatmap.DifficultyName}]");

            //Create loggers
            LogTracker.AddLogger("DeltaTime", Color.LawnGreen);
            LogTracker.AddLogger("SongTime", Color.White);
            LogTracker.AddLogger("SongPos", Color.White);
            LogTracker.AddLogger("HitObjects", Color.Wheat);
            LogTracker.AddLogger("Skippable", CustomColors.NameTagAdmin);
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

            // Check if the song is currently skippable.
            SongSkippable = (Qua.HitObjects[0].StartTime - _currentSongTime >= 5000);

            // Update the Notes
            UpdateNotes(dt);

            // Check the input for this particular game state.
            InputManager.CheckInput(Qua, SongSkippable);

            // Update Loggers
            LogTracker.UpdateLogger("DeltaTime", "Delta Time: " + dt + "ms");
            LogTracker.UpdateLogger("SongTime", "Current Song Time: " + _currentSongTime + "ms");
            LogTracker.UpdateLogger("SongPos", "Current Track Position: " + _trackPosition);
            LogTracker.UpdateLogger("HitObjects", "Total Remaining Notes: " + _hitObjectQueue.Count);
            LogTracker.UpdateLogger("Skippable", $"Song Skippable: {SongSkippable}");
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