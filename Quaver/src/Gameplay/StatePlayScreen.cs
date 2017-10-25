﻿using System;
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
    internal class StatePlayScreen : IGameState
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
        ///     The MD5 Hash of the played beatmap.
        /// </summary>
        private string BeatmapMd5 { get; set; }

        /// <summary>
        ///     Keeps track of whether or not the song intro is current skippable.
        /// </summary>
        private bool IntroSkippable { get; set; }

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

            // Get beatmap MD5 of the parsed qua.
            BeatmapMd5 = BeatmapUtils.GetMd5Checksum(GameBase.SelectedBeatmap.Path);

            Console.WriteLine($"[GAMEPLAY STATE] Initialized Gameplay State with Mods: { String.Join(", ", GameBase.CurrentGameModifiers.Select(x => x.ModIdentifier)) }");
            Console.WriteLine($"[GAMEPLAY STATE] Loaded Beatmap: {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title} [{GameBase.SelectedBeatmap.DifficultyName}]");
            Console.WriteLine($"[GAMEPLAY STATE] Loaded Beatmap MD5: {BeatmapMd5}");

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
            Timing.InitializeTiming(Qua);
            NoteRendering.InitializeNotes(Qua);
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void UnloadContent()
        {
            //Do unload stuff
            //GameStateManager.Instance.UnloadContent();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Get the current game time in milliseconds.
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Set the current song time.
            Timing.SetCurrentSongTime(dt);

            // Update the playfield
            Playfield.UpdatePlayfield(dt); ;

            // Check if the song is currently skippable.
            IntroSkippable = (Qua.HitObjects[0].StartTime - Timing.CurrentSongTime >= 5000);

            // Update the Notes
            NoteRendering.UpdateNotes(dt);

            // Check the input for this particular game state.
            InputManager.CheckInput(Qua, IntroSkippable);

            // Update Loggers
            LogTracker.UpdateLogger("DeltaTime", "Delta Time: " + dt + "ms");
            LogTracker.UpdateLogger("SongTime", "Current Song Time: " + Timing.CurrentSongTime + "ms");
            LogTracker.UpdateLogger("SongPos", "Current Track Position: " + NoteRendering.TrackPosition);
            LogTracker.UpdateLogger("HitObjects", "Total Remaining Notes: " + NoteRendering.HitObjectQueue.Count);
            LogTracker.UpdateLogger("Skippable", $"Intro Skippable: {IntroSkippable}");
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