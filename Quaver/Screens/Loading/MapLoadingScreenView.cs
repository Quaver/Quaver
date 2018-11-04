using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Screens.Gameplay;
using Quaver.Screens.Menu;
using Wobble;
using Wobble.Audio;
using Wobble.Logging;
using Wobble.Screens;

namespace Quaver.Screens.Loading
{
    public class MapLoadingScreenView : ScreenView
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MapLoadingScreenView(Screen screen) : base(screen) => Task.Run(() => ParseAndLoadMap()).ContinueWith((t) => LoadGameplayScreen());

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        ///     Loads the currently selected map asynchronously.
        /// </summary>
        private static void ParseAndLoadMap()
        {
            try
            {
                // Throw an exception if there is no selected map.
                if (MapManager.Selected.Value == null)
                    throw new Exception("No selected map, we should not be on this screen!");

                MapManager.Selected.Value.Qua = MapManager.Selected.Value.LoadQua();

                // Asynchronously write to a file for livestreamers the difficulty rating
                using (var writer = File.CreateText(ConfigManager.DataDirectory + "/temp/Now Playing/difficulty.txt"))
                    writer.Write($"{Math.Round(MapManager.Selected.Value.Qua.AverageNotesPerSecond(), 2)}");
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Loads the gameplay screen (audio, md5 hashes, etc.)
        /// </summary>
        private static void LoadGameplayScreen()
        {
            try
            {
                // Stop the current audio and load it again before moving onto the next state.
                try
                {
                    AudioEngine.LoadCurrentTrack();
                }
                catch (AudioEngineException e)
                {
                    Console.WriteLine(e);
                    Logger.Warning("Audio file could not be loaded, but proceeding anyway!", LogType.Runtime);
                }

                // Get the MD5 Hash of the played map and change the state.
                var quaPath = $"{ConfigManager.SongDirectory}/{MapManager.Selected.Value.Directory}/{MapManager.Selected.Value.Path}";

                // Get the Md5 of the played map
                string md5;
                switch (MapManager.Selected.Value.Game)
                {
                    case MapGame.Quaver:
                        md5 = MapsetHelper.GetMd5Checksum(quaPath);
                        break;
                    case MapGame.Osu:
                        md5 = MapsetHelper.GetMd5Checksum($"{MapManager.OsuSongsFolder}/{MapManager.Selected.Value.Directory}/{MapManager.Selected.Value.Path}");
                        break;
                    case MapGame.Etterna:
                        // Etterna uses some weird ChartKey system, no point in implementing that here.
                        md5 = MapManager.Selected.Value.Md5Checksum;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                QuaverScreenManager.ChangeScreen(new GameplayScreen(MapManager.Selected.Value.Qua, md5, new List<LocalScore>()));
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }
    }
}
