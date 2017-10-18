using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Quaver.Audio;
using Quaver.Beatmaps;
using Quaver.GameState;
using Quaver.QuaFile;

namespace Quaver.Gameplay
{
    /// <summary>
    ///     This is the GameState when the player is actively playing.
    /// </summary>
    internal partial class StatePlayScreen : GameStateBase
    {
        //Beatmap
        //private Beatmap _beatmap;
        private GameAudio _gameAudio;
        private Qua _qua;

        private readonly string _TESTAUDIODIRECTORY =
            $@"{
                    Path.GetFullPath(
                        @"..\..\..\Test\Beatmaps\2. Camellia - Backbeat Maniac\audio.ogg")
                }";

        private HitObject[] _testHitObject = new HitObject[4];

        //TEST
        private readonly string _TESTMAPDIRECTORY =
            $@"{
                    Path.GetFullPath(
                        @"..\..\..\Test\Beatmaps\2. Camellia - Backbeat Maniac\Camellia - Backbeat Maniac () [Rewind VIP].qua")
                }";

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
            //GameBase.SelectRandomBeatmap();
            //_Beatmap = GameBase.SelectedBeatmap;
            //Console.WriteLine("Loaded Beatmap: {0} - {1}",_Beatmap.Artist,_Beatmap.Title);
            _qua = new Qua(_TESTMAPDIRECTORY);
            //_GameAudio = new GameAudio(_Qua.AudioFile);
            _gameAudio = new GameAudio(_TESTAUDIODIRECTORY);
            Console.WriteLine("Loaded Beatmap: {0} - {1}", _qua.Artist, _qua.Title);
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
            //_gameAudio.Play();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            GameStateManager.Instance.UnloadContent();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            SetCurrentSongTime(dt);
            UpdatePlayField(dt);
            UpdateNotes(dt);
            CheckInput();
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