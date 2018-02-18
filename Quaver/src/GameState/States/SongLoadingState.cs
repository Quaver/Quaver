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
using Quaver.API.StepMania;
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
                switch (GameBase.SelectedBeatmap.Game)
                {
                    case BeatmapGame.Quaver:
                        var quaPath = $"{Configuration.SongDirectory}/{GameBase.SelectedBeatmap.Directory}/{GameBase.SelectedBeatmap.Path}";
                        qua = Qua.Parse(quaPath);
                        break;
                    case BeatmapGame.Osu:
                        var osu = new PeppyBeatmap(GameBase.OsuSongsFolder + GameBase.SelectedBeatmap.Directory + "/" + GameBase.SelectedBeatmap.Path);
                        qua = Qua.ConvertOsuBeatmap(osu);
                        break;
                    case BeatmapGame.Etterna:
                        // In short, find the chart with the same DifficultyName. There's literally no other way for us to check
                        // other than through this means.
                        var smCharts = Qua.ConvertStepManiaChart(StepManiaFile.Parse(GameBase.EtternaFolder + GameBase.SelectedBeatmap.Directory + "/" + GameBase.SelectedBeatmap.Path));
                        qua = smCharts.Find(x => x.DifficultyName == GameBase.SelectedBeatmap.DifficultyName);
                        break;
                    default:
                        throw new ArgumentException("Could not load map because BeatmapGame is invalid");
                }

                // Check if the map is actually valid
                qua.IsValidQua = Qua.CheckQuaValidity(qua);
                if (!qua.IsValidQua)
                {
                    Logger.LogError("Beatmap could not be loaded!", LogType.Runtime);
                    return;
                }
                    
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

                Logger.LogSuccess("Finished loading Beatmap", LogType.Runtime);
                GameBase.SelectedBeatmap.Qua.CalculateDifficulty(GameBase.AudioEngine.PlaybackRate);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Responsible for getting the state ready to be changed, and then actually changing it.
        /// </summary>
        private void ChangeState()
        {
            // If the Qua isn't valid then return back to the song select state
            if (!GameBase.SelectedBeatmap.Qua.IsValidQua)
            {
                GameBase.GameStateManager.ChangeState(new SongSelectState());
                return;
            }

            try
            {
                // Stop the current audio and load it again before moving onto the next state.
                try
                {
                    GameBase.AudioEngine.Stop();
                    GameBase.AudioEngine.Load();
                } catch (AudioEngineException e)
                {
                    Logger.LogWarning("Audio file could not be loaded, but proceeding anyway!", LogType.Runtime);
                }

                // Get the MD5 Hash of the played map and change the state.
                var quaPath = $"{Configuration.SongDirectory}/{GameBase.SelectedBeatmap.Directory}/{GameBase.SelectedBeatmap.Path}";

                // Get the Md5 of the played map
                var md5 = "";
                switch (GameBase.SelectedBeatmap.Game)
                {
                    case BeatmapGame.Quaver:
                        md5 = BeatmapUtils.GetMd5Checksum(quaPath);
                        break;
                    case BeatmapGame.Osu:
                        md5 = BeatmapUtils.GetMd5Checksum($"{GameBase.OsuSongsFolder}/{GameBase.SelectedBeatmap.Directory}/{GameBase.SelectedBeatmap.Path}");
                        break;
                    case BeatmapGame.Etterna:
                        // Etterna uses some weird ChartKey system, no point in implementing that here.
                        md5 = GameBase.SelectedBeatmap.Md5Checksum;
                        break;
                }

                GameBase.GameStateManager.ChangeState(new PlayScreenState(GameBase.SelectedBeatmap.Qua, md5));
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }
    }
}
