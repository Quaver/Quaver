using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Beatmaps;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Input;
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
        ///     The Audio, used for testing purposes (We'll use this on the Beatmap class objecvt itself later.)
        /// </summary>
        private GameAudio TestSong { get; set; }

        /// <summary>
        ///     The Qua object - Parsed .qua file.
        /// </summary>
        private Qua Qua{ get; set; }

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

        //TEST
        private double prevSongTime = 0;

        private double curSongTime = 0;

        private double prevByteTime = 0;
        private double curByteTime = 0;

        private double averageSongTime = 0;
        private double averageByteTime = 0;

        private bool alreadyTested = false;

        private int frameCount = 0;

        private void Tester()
        {

            if (!alreadyTested)
            {
                prevSongTime = _currentSongTime;
                prevByteTime = TestSong.GetAudioPosition();
                alreadyTested = true;
            }
            frameCount++;
            curByteTime = TestSong.GetAudioPosition();
            curSongTime = _currentSongTime;

            averageSongTime += Math.Pow(curSongTime-prevSongTime,4);
            averageByteTime += Math.Pow(curByteTime-prevByteTime,4);

            LogTracker.QuickLog("SongTime Unsteadiness: "+string.Format("{0:0.00}", averageSongTime/frameCount));
            LogTracker.QuickLog("GetAudioPosition() Unsteadiness: " + string.Format("{0:0.00}", averageByteTime / frameCount));

            prevSongTime = curSongTime;
            prevByteTime = curByteTime;
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Initialize()
        {
            //Load Qua + Audio
            Console.WriteLine("[STATE_PLAYSCREEN]: Initialized Gameplay State.");

            // Set .qua and audio - The qua should be parsed from the Beatmap class object path, and the song will be auto loaded.
            // but this is ok for testing purposes.
            Qua = new Qua(Path.GetFullPath(@"..\..\..\Test\Beatmaps\2. Camellia - Backbeat Maniac\Camellia - Backbeat Maniac () [Rewind VIP].qua"));
            TestSong = new GameAudio(Path.GetFullPath(@"..\..\..\Test\Beatmaps\2. Camellia - Backbeat Maniac\audio.ogg"));

            Console.WriteLine("Loaded Beatmap: {0} - {1}", Qua.Artist, Qua.Title);

            //Create loggers
            LogTracker.AddLogger("DeltaTime",Color.LawnGreen);
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
        public  void Update(GameTime gameTime)
        {
            // Get the current game time in milliseconds.
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Set the current song time.
            SetCurrentSongTime(dt);

            // Update the playfield
            Playfield.UpdatePlayfield(dt);;

            // Update the Notes
            UpdateNotes(dt);

            // Check the input for this particular game state.
            InputManager.CheckInput();

            // Update Loggers
            LogTracker.UpdateLogger("DeltaTime", "Delta Time: " + dt + "ms");
            LogTracker.UpdateLogger("SongTime", "Current Song Time: " + _currentSongTime + "ms");
            LogTracker.UpdateLogger("SongPos", "Current Track Position: " + _trackPosition);
            LogTracker.UpdateLogger("HitObjects", "Total Remaining Notes: " + _hitObjectQueue.Count);

            if (_currentSongTime > 10)
            Tester();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Draw()
        {
            Playfield.PlayfieldBoundary.Draw();
        }
    }
}