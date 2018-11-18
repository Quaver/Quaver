using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Scheduling;
using Quaver.Screens.Gameplay;
using Quaver.Server.Common.Objects;
using Wobble.Audio;
using Wobble.Audio.Tracks;
using Wobble.Logging;
using Wobble.Screens;
using AudioEngine = Quaver.Audio.AudioEngine;

namespace Quaver.Screens.Loading
{
    public class MapLoadingScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Loading;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     The local scores from the leaderboard that'll be used during gameplay.
        /// </summary>
        private List<LocalScore> Scores { get; }

        /// <summary>
        /// </summary>
        public MapLoadingScreen(List<LocalScore> scores)
        {
            Scores = scores;
            View = new MapLoadingScreenView(this);
            AudioTrack.AllowPlayback = false;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            ThreadScheduler.Run(() =>
            {
                ParseAndLoadMap();
                LoadGameplayScreen();
            });

            base.OnFirstUpdate();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => null;

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
        private void LoadGameplayScreen()
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

                Exit(() => new GameplayScreen(MapManager.Selected.Value.Qua, md5, new List<LocalScore>()));
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }
    }
}
