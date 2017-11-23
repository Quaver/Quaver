using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Audio;
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
        ///     Try to load the qua file and song. 
        ///     If we've successfully loaded it, move onto the play state.
        /// </summary>
        public void Initialize()
        {
            Task.Run(() => LoadBeatmap()).ContinueWith(t => ChangeState());
        }

        /// <summary>
        ///     Unload Content
        /// </summary>
        public void UnloadContent() { }

        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(double dt)
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

                Logger.Log("Finished loading Beatmap", Color.Cyan);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.TargetSite + "\n" + ex.StackTrace + "\n" + ex.Message + "\n", Color.Red, 5.0f);
            }
        }

        /// <summary>
        ///     Responsible for getting the state ready to be changed, and then actually changing it.
        /// </summary>
        private void ChangeState()
        {
            try
            {
                // Stop the current audio and load it again before moving onto the next state.
                SongManager.Stop();
                SongManager.Load();

                // Detect if the audio can't be played.
                if (SongManager.Length < 1)
                    throw new Exception("[SONG LOADING STATE] Audio file could not be loaded.");

                // Get the MD5 Hash of the played map and change the state.
                var quaPath = $"{Config.Configuration.SongDirectory}/{GameBase.SelectedBeatmap.Directory}/{GameBase.SelectedBeatmap.Path}";
                GameBase.GameStateManager.ChangeState(new PlayScreenState(BeatmapUtils.GetMd5Checksum(quaPath)));

                Logger.Log("Successfully changed to the gameplay state.", Color.Cyan);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.TargetSite + "\n" + ex.StackTrace + "\n" + ex.Message + "\n", Color.Red, 5.0f);
            }
        }
    }
}
