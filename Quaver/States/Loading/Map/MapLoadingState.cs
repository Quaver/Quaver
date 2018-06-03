using System;
using System.IO;
using System.Threading.Tasks;
using Quaver.API.Maps;
using Quaver.API.Maps.Parsers;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.GameState;
using Quaver.Logging;
using Quaver.Main;
using Quaver.States.Enums;
using Quaver.States.Gameplay;
using Quaver.States.Select;

namespace Quaver.States.Loading.Map
{
    internal class MapLoadingState : IGameState
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
            Task.Run(() => LoadMap()).ContinueWith(t => ChangeState());
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
        ///     Responsible for loading the map and switching the state.
        /// </summary>
        private void LoadMap()
        {
            try
            {
                // Throw an exception if there is no selected map.
                if (GameBase.SelectedMap == null)
                    throw new Exception("No selected map, we should not be on this screen!!!");

                // Reference to the parsed .qua file
                var qua = new Qua();

                // Handle osu! maps as well
                switch (GameBase.SelectedMap.Game)
                {
                    case MapGame.Quaver:
                        var quaPath = $"{ConfigManager.SongDirectory}/{GameBase.SelectedMap.Directory}/{GameBase.SelectedMap.Path}";
                        qua = Qua.Parse(quaPath);
                        break;
                    case MapGame.Osu:
                        var osu = new PeppyBeatmap(GameBase.OsuSongsFolder + GameBase.SelectedMap.Directory + "/" + GameBase.SelectedMap.Path);
                        qua = Qua.ConvertOsuBeatmap(osu);
                        break;
                    case MapGame.Etterna:
                        // In short, find the chart with the same DifficultyName. There's literally no other way for us to check
                        // other than through this means.
                        var smCharts = Qua.ConvertStepManiaChart(StepManiaFile.Parse(GameBase.EtternaFolder + GameBase.SelectedMap.Directory + "/" + GameBase.SelectedMap.Path));
                        qua = smCharts.Find(x => x.DifficultyName == GameBase.SelectedMap.DifficultyName);
                        break;
                    default:
                        throw new ArgumentException("Could not load map because MapGame is invalid");
                }

                // Check if the map is actually valid
                if (qua != null)
                    qua.IsValidQua = Qua.CheckQuaValidity(qua);

                if (qua == null || !qua.IsValidQua)
                {
                    Logger.LogError("Map could not be loaded!", LogType.Runtime);
                    GameBase.GameStateManager.ChangeState(new SongSelectState());
                    return;
                }
                    
                // Set the map's Qua. 
                // We parse it and set it each time the player is going to play to kmake sure they are
                // actually playing the correct map.
                GameBase.SelectedMap.Qua = qua;

                // Asynchronously write to a file for livestreamers the difficulty rating
                Task.Run(async () =>
                {
                    using (var writer = File.CreateText(ConfigManager.DataDirectory + "/temp/Now Playing/difficulty.txt"))
                        await writer.WriteAsync($"{Math.Round(GameBase.SelectedMap.Qua.CalculateFakeDifficulty(), 2)}");
                });

                Logger.LogSuccess("Finished loading Map", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Responsible for getting the state ready to be changed, and then actually changing it.
        /// </summary>
        private static void ChangeState()
        {
            // If the Qua isn't valid then return back to the song select state
            if (!GameBase.SelectedMap.Qua.IsValidQua)
            {
                GameBase.GameStateManager.ChangeState(new SongSelectState());
                return;
            }

            try
            {
                // Stop the current audio and load it again before moving onto the next state.
                try
                {
                    GameBase.AudioEngine.Load();
                } catch (AudioEngineException e)
                {
                    Console.WriteLine(e);
                    Logger.LogWarning("Audio file could not be loaded, but proceeding anyway!", LogType.Runtime);
                }

                // Get the MD5 Hash of the played map and change the state.
                var quaPath = $"{ConfigManager.SongDirectory}/{GameBase.SelectedMap.Directory}/{GameBase.SelectedMap.Path}";

                // Get the Md5 of the played map
                var md5 = "";
                switch (GameBase.SelectedMap.Game)
                {
                    case MapGame.Quaver:
                        md5 = MapsetHelper.GetMd5Checksum(quaPath);
                        break;
                    case MapGame.Osu:
                        md5 = MapsetHelper.GetMd5Checksum($"{GameBase.OsuSongsFolder}/{GameBase.SelectedMap.Directory}/{GameBase.SelectedMap.Path}");
                        break;
                    case MapGame.Etterna:
                        // Etterna uses some weird ChartKey system, no point in implementing that here.
                        md5 = GameBase.SelectedMap.Md5Checksum;
                        break;
                }
            
                // GameBase.GameStateManager.ChangeState(new ManiaGameplayState(GameBase.SelectedMap.Qua, md5));
                GameBase.GameStateManager.ChangeState(new GameplayScreen(GameBase.SelectedMap.Qua, md5));
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }
    }
}
