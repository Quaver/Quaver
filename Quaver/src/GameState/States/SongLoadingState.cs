using System;
using System.Collections.Generic;
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

        /// <summary>
        ///     Try to load the qua file and song. 
        ///     If we've successfully loaded it, move onto the play state.
        /// </summary>
        public void Initialize()
        {
            try
            {
                Console.WriteLine("[SONG LOADING STATE] Attempting to load beatmap...");

                // Throw an exception if there is no selected beatmap.
                if (GameBase.SelectedBeatmap == null)
                    throw new Exception("No selected beatmap, we should not be on this screen!!!");

                // Try and parse the .qua and check if it is valid.
                var qua = new Qua(GameBase.SelectedBeatmap.Path);

                if (!qua.IsValidQua)
                    throw new Exception("[SONG LOADING STATE] The .qua file could NOT be loaded!");

                Console.WriteLine("[SONG LOADING STATE] Qua successfully loaded.");

                // Attempt to load the audio.
                GameBase.SelectedBeatmap.LoadAudio();

                if (GameBase.SelectedBeatmap.Song.GetAudioLength() < 1)
                    throw new Exception("[SONG LOADING STATE] Audio file could not be loaded.");

                Console.WriteLine("[SONG LOADING STATE] Audio file successfully loaded.");

                // Get Beatmap MD5
                var md5 = BeatmapUtils.GetMd5Checksum(GameBase.SelectedBeatmap.Path);
                Console.WriteLine($"[SONG LOADING STATE] Successfully loaded beatmap: {md5}");

                // Load Play State
                GameStateManager.Instance.ChangeState(new PlayScreenState(qua, md5));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);   
            }
        }

        public void LoadContent() {}

        public void UnloadContent() { }

        public void Update(GameTime gameTime) {}

        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.Red);
        }
    }
}
