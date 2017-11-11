using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Beatmaps;
using Quaver.Gameplay;
using Quaver.Main;
using Quaver.QuaFile;

namespace Quaver.GameState.States
{
    internal class SongLoadingState : IGameState
    {
        public State CurrentState { get; set; } = State.LoadingScreen;
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     Try to load the qua file and song. 
        ///     If we've successfully loaded it, move onto the play state.
        /// </summary>
        public void Initialize()
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
                //Console.WriteLine("[SONG LOADING STATE] Qua successfully loaded.");

                // Attempt to load the audio.
                GameBase.SelectedBeatmap.LoadAudio();

                if (GameBase.SelectedBeatmap.Song.GetAudioLength() < 1)
                    throw new Exception("[SONG LOADING STATE] Audio file could not be loaded.");

                // Get Beatmap MD5
                var md5 = BeatmapUtils.GetMd5Checksum(quaPath);
                //Console.WriteLine($"[SONG LOADING STATE] Successfully loaded beatmap: {md5}");

                // Load Play State
                GameStateManager.Instance.ChangeState(new PlayScreenState(md5));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void LoadContent()
        {
            /*try
            {
                Console.WriteLine("[SONG LOADING STATE] Attempting to load beatmap...");

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
                Console.WriteLine("[SONG LOADING STATE] Qua successfully loaded.");

                // Attempt to load the audio.
                GameBase.SelectedBeatmap.LoadAudio();

                if (GameBase.SelectedBeatmap.Song.GetAudioLength() < 1)
                    throw new Exception("[SONG LOADING STATE] Audio file could not be loaded.");

                Console.WriteLine("[SONG LOADING STATE] Audio file successfully loaded.");

                // Get Beatmap MD5
                var md5 = BeatmapUtils.GetMd5Checksum(quaPath);
                Console.WriteLine($"[SONG LOADING STATE] Successfully loaded beatmap: {md5}");

                // Load Play State
                GameStateManager.Instance.ChangeState(new PlayScreenState(md5));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }*/
        }

        public void UnloadContent() { }

        public void Update(GameTime gameTime) {}

        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.Red);
        }
    }
}
