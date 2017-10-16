using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Quaver.GameState;
using Quaver.Utility;
using Quaver.Graphics;
using Quaver.Main;

using Quaver.Beatmaps;
using Quaver.QuaFile;
using Quaver.Audio;

namespace Quaver.Gameplay
{
    /// <summary>
    /// This is the GameState when the player is actively playing.
    /// </summary>
    internal partial class StatePlayScreen : GameStateBase
    {
        //Beatmap
        private Beatmap _Beatmap;
        private Qua _Qua;
        private GameAudio _GameAudio;

        public StatePlayScreen()
        {
            //Important to assign a state to this class.
            CurrentState = State.PlayScreen;
        }

        //TEST
        private string _TESTMAPDIRECTORY = $@"{Path.GetFullPath(@"..\..\..\Test\Beatmaps\672208 Camellia - shadows of cats\Camellia - shadows of cats (Applequestria) [Dreamless Wanderer].qua")}";
        private string _TESTAUDIODIRECTORY = $@"{Path.GetFullPath(@"..\..\..\Test\Beatmaps\672208 Camellia - shadows of cats\Camellia - shadows of cats.mp3")}";
        private HitObject[] _testHitObject = new HitObject[4];
        private int _testNoteSize = 67;

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
            _Qua = new Qua(_TESTMAPDIRECTORY);
            //_GameAudio = new GameAudio(_Qua.AudioFile);
            _GameAudio = new GameAudio(_TESTAUDIODIRECTORY);
            Console.WriteLine("Loaded Beatmap: {0} - {1}", _Qua.Artist, _Qua.Title);

            //Initialize Components
            InitializePlayField();
            InitializeNotes();
            InitializeTiming();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void LoadContent()
        {
            //Initialize
            _GameAudio.Play();
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
            double dt = gameTime.ElapsedGameTime.TotalSeconds;
            UpdateTiming(dt);
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
