﻿using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Beatmaps;
using Quaver.GameState;
using Quaver.Input;
using Quaver.QuaFile;

namespace Quaver.Gameplay
{
    /// <summary>
    ///     This is the GameState when the player is actively playing.
    /// </summary>
    internal partial class StatePlayScreen : GameStateBase
    {
        /// <summary>
        ///     The input manager for this game state.
        /// </summary>
        private GameplayInputManager InputManager { get; } = new GameplayInputManager();

        /// <summary>
        ///     The Audio, used for testing purposes (We'll use this on the Beatmap class objecvt itself later.)
        /// </summary>
        private GameAudio TestSong { get; set; }

        /// <summary>
        ///     The Qua object
        /// </summary>
        private Qua Qua{ get; set; }

        // Ctor
        public StatePlayScreen()
        {
            //Important to assign a state to this class.
            CurrentState = State.PlayScreen;
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void Initialize()
        {
            //Load Qua + Audio
            Console.WriteLine("[STATE_PLAYSCREEN]: Initialized Gameplay State.");

            // Set .qua and audio - The qua should be parsed from the Beatmap class object path, and the song will be auto loaded.
            // but this is ok for testing purposes.
            Qua = new Qua(Path.GetFullPath(@"..\..\..\Test\Beatmaps\2. Camellia - Backbeat Maniac\Camellia - Backbeat Maniac () [Rewind VIP].qua"));
            TestSong = new GameAudio(Path.GetFullPath(@"..\..\..\Test\Beatmaps\2. Camellia - Backbeat Maniac\audio.ogg"));

            Console.WriteLine("Loaded Beatmap: {0} - {1}", Qua.Artist, Qua.Title);
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void LoadContent()
        {
            //Initialize Components
            InitializeConfig();
            InitializePlayField();
            InitializeTiming();
            InitializeNotes();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void UnloadContent()
        {
            GameStateManager.Instance.UnloadContent();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Get the current game time in milliseconds.
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Set the current song time.
            SetCurrentSongTime(dt);

            // Update the playfield
            UpdatePlayField(dt);

            // Update the Notes
            UpdateNotes(dt);

            // Check the input for this particular game state.
            InputManager.CheckInput();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void Draw()
        {
            _PlayField.Draw();
        }
    }
}