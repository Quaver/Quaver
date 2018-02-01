using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Database.Beatmaps;
using Quaver.Logging;
using Quaver.Replays;
using Quaver.API.Maps;
using Quaver.API.Osu;
using Quaver.Config;

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
            // todo: Implement map loading drawing
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

                // Reference to the parsed .qua file
                var qua = new Qua();

                // Handle osu! beatmaps as well
                if (GameBase.SelectedBeatmap.IsOsuMap)
                {
                    var osu = new PeppyBeatmap(GameBase.OsuSongsFolder + GameBase.SelectedBeatmap.Directory + "/" + GameBase.SelectedBeatmap.Path);
                    qua = Qua.ConvertOsuBeatmap(osu);
                }
                else
                {
                    var quaPath = $"{Configuration.SongDirectory}/{GameBase.SelectedBeatmap.Directory}/{GameBase.SelectedBeatmap.Path}";
                    qua = Qua.Parse(quaPath);
                }

                // Check if the map is actually valid
                if (!qua.IsValidQua)
                    throw new Exception("[SONG LOADING STATE] The .qua file could NOT be loaded!");

                // Set the beatmap's Qua. 
                // We parse it and set it each time the player is going to play to kmake sure they are
                // actually playing the correct map.
                GameBase.SelectedBeatmap.Qua = qua;

                // Asynchronously write to a file for livestreamers the difficulty rating
                Task.Run(async () =>
                {
                    using (var writer = File.CreateText(Configuration.DataDirectory + "/temp/Now Playing/difficulty.txt"))
                        await writer.WriteAsync($"{Math.Round(GameBase.SelectedBeatmap.Qua.CalculateFakeDifficulty(), 2)}");
                });

                Logger.Log("Finished loading Beatmap", LogColors.GameSuccess);
                GameBase.SelectedBeatmap.Qua.CalculateDifficulty(GameBase.GameClock);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.TargetSite + "\n" + ex.StackTrace + "\n" + ex.Message + "\n", LogColors.GameError, 5.0f);
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
                var quaPath = $"{Configuration.SongDirectory}/{GameBase.SelectedBeatmap.Directory}/{GameBase.SelectedBeatmap.Path}";

                // Get the MD5 of the map even if it's an osu! file.
                var md5 = (GameBase.SelectedBeatmap.IsOsuMap) ? 
                        BeatmapUtils.GetMd5Checksum($"{GameBase.OsuSongsFolder}/{GameBase.SelectedBeatmap.Directory}/{GameBase.SelectedBeatmap.Path}")
                        : BeatmapUtils.GetMd5Checksum(quaPath);
   

                GameBase.GameStateManager.ChangeState(new PlayScreenState(GameBase.SelectedBeatmap.Qua, md5));
                Logger.Log("Successfully changed to the gameplay state.", LogColors.GameSuccess);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.TargetSite + "\n" + ex.StackTrace + "\n" + ex.Message + "\n", LogColors.GameError, 5.0f);
            }
        }
    }
}
