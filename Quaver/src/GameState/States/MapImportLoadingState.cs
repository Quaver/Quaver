using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Database.Beatmaps;
using Quaver.Discord;
using Quaver.Graphics.Sprite;
using Quaver.Logging;
using Quaver.Maps;

namespace Quaver.GameState.States
{
    internal class MapImportLoadingState : IGameState
    {
        /// <summary>
        ///     The current state
        /// </summary>
        public State CurrentState { get; set; } = State.LoadingScreen;

        /// <summary>
        ///     Update Ready?
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     Initialize
        /// </summary>
        public void Initialize()
        {
            // TODO: Add some sort of general loading screen here. The state is only going to be used during map importing.
            // Set Rich Presence
            GameBase.ChangeDiscordPresence("Importing maps", "In the menus");
        }

        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent() { }

        /// <summary>
        ///     Update
        /// </summary>
        public void Update(double dt) { }

        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.Red);
        }


        /// <summary>
        ///     Handles the selection of new a new beatmap and 
        ///     removes the loading state after importing
        /// </summary>
        /// <returns></returns>
        internal static async Task AfterImport()
        {
            var oldMaps = GameBase.Mapsets;

            // Import all the maps to the db
            await GameBase.LoadAndSetBeatmaps();

            // Update the selected beatmap with the new one.
            // This button should only be on the song select state, so no need to check for states here.
            var newMapsets = GameBase.Mapsets.Where(x => !oldMaps.Any(y => y.Directory == x.Directory)).ToList();

            // In the event that the user imports maps when there weren't any maps previously.
            if (oldMaps.Count == 0)
            {
                BeatmapUtils.SelectRandomBeatmap();
                GameBase.LoadBackground();
                BackgroundManager.Change(GameBase.CurrentBackground);
                SongManager.Load();
                SongManager.Play();
            }
            else if (newMapsets.Count > 0)
            {
                var map = newMapsets.Last().Beatmaps.Last();
                Console.WriteLine(map.Artist + " " + map.Title);

                // Switch map and load audio for song and play it.
                GameBase.ChangeBeatmap(map);

                // Load and change background after import
                GameBase.LoadBackground();
                BackgroundManager.Change(GameBase.CurrentBackground);

                SongManager.ReloadSong();

                GameBase.ChangeDiscordPresence(
                    $"{GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title}", "Listening");
            }

            Logger.Log("Successfully completed the conversion task. Stopping loader.", Color.Cyan);
            GameBase.GameStateManager.RemoveState();
        }
    }
}
