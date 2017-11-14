using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Beatmaps;
using Quaver.Gameplay;
using Quaver.Logging;
using Quaver.QuaFile;

namespace Quaver.GameState.States
{
    internal class SongLoadingState : IGameState
    {
        /// <summary>
        ///     Current State
        /// </summary>
        public State CurrentState { get; set; } = State.LoadingScreen;

        /// <summary>
        ///     Update Ready
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     Keeps track of if we're currently loading the map.
        /// </summary>
        private bool IsLoading { get; set; }

        /// <summary>
        ///     Try to load the qua file and song. 
        ///     If we've successfully loaded it, move onto the play state.
        /// </summary>
        public void Initialize()
        {
            LoadBeatmap();
            InitializeGameplay(GameBase.SelectedBeatmap.Qua);
            ChangeState();
        }

        /// <summary>
        ///     Unload Content
        /// </summary>
        public void UnloadContent() { }

        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
        }

        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            //GameBase.GraphicsDevice.Clear(Color.Red);
        }

        /// <summary>
        ///     Responsible for loading the beatmap and switching the state.
        /// </summary>
        private void LoadBeatmap()
        {
            try
            {
                // Throw an exception if there is no selected beatmap.
                if (GameBase.SelectedBeatmap == null)
                    throw new Exception("No selected beatmap, we should not be on this screen!!!");

                // Try and parse the .qua and check if it is valid.
                var quaPath = $"{Config.Configuration.SongDirectory}/{GameBase.SelectedBeatmap.Directory}/{GameBase.SelectedBeatmap.Path}";

                var qua = new Qua(quaPath);

                if (!qua.IsValidQua)
                    throw new Exception("[SONG LOADING STATE] The .qua file could NOT be loaded!");

                // Set the beatmap's Qua. 
                // We parse it and set it each time the player is going to play to kmake sure they are
                // actually playing the correct map.
                GameBase.SelectedBeatmap.Qua = qua;

                // Attempt to load the audio.
                GameBase.SelectedBeatmap.Song.Stop();
                GameBase.SelectedBeatmap.LoadAudio();

                // Detect if the audio can't be played.
                if (GameBase.SelectedBeatmap.Song.GetAudioLength() < 1)
                    throw new Exception("[SONG LOADING STATE] Audio file could not be loaded.");

                LogManager.ConsoleLog("[SONG LOADING STATE]: Done Loading Beatmap", ConsoleColor.DarkBlue);
            }
            catch (Exception ex)
            {
                LogManager.Debug(ex.TargetSite + "\n" + ex.StackTrace + "\n" + ex.Message + "\n");
            }
        }

        /// <summary>
        ///     Initializes gameplay classes
        /// </summary>
        /// <param name="qua"></param>
        private void InitializeGameplay(Qua qua)
        {
            try
            {
                Playfield.Initialize();
                Timing.Initialize(qua);
                NoteRendering.Initialize(qua);
                LogManager.ConsoleLog("[SONG LOADING STATE]: Done Initializing Gameplay", ConsoleColor.DarkBlue);
            }
            catch (Exception ex)
            {
                LogManager.Debug(ex.TargetSite + "\n" + ex.StackTrace + "\n" + ex.Message + "\n");
            }
        }

        private void ChangeState()
        {
            try
            {
                var quaPath = $"{Config.Configuration.SongDirectory}/{GameBase.SelectedBeatmap.Directory}/{GameBase.SelectedBeatmap.Path}";
                var md5 = BeatmapUtils.GetMd5Checksum(quaPath);
                GameBase.GameStateManager.ChangeState(new PlayScreenState(md5));
                LogManager.ConsoleLog("[SONG LOADING STATE]: Done Changing States", ConsoleColor.DarkBlue);
            }
            catch (Exception ex)
            {
                LogManager.Debug(ex.TargetSite + "\n" + ex.StackTrace + "\n" + ex.Message + "\n");
            }
        }
    }
}
