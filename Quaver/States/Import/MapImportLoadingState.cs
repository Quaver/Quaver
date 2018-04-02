using System;
using System.Linq;
using System.Threading.Tasks;
using Quaver.Database.Maps;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Logging;
using Quaver.Main;
using Quaver.States.Enums;

namespace Quaver.States.Import
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
            DiscordController.ChangeDiscordPresence("Importing maps", "In the menus");
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
            // todo: Implement loading state drawing
            /*
            GameBase.SpriteBatch.Begin();
            GameBase.GraphicsDevice.Clear(Color.Red);
            GameBase.SpriteBatch.End();
            */
        }


        /// <summary>
        ///     Handles the selection of new a new map and 
        ///     removes the loading state after importing
        /// </summary>
        /// <returns></returns>
        internal static async Task AfterImport()
        {
            var oldMaps = GameBase.Mapsets;

            // Import all the maps to the db
            await MapCache.LoadAndSetMapsets();

            // Update the selected map with the new one.
            // This button should only be on the song select state, so no need to check for states here.
            var newMapsets = GameBase.Mapsets.Where(x => !oldMaps.Any(y => y.Directory == x.Directory)).ToList();

            // In the event that the user imports maps when there weren't any maps previously.
            if (oldMaps.Count == 0)
            {
            }
            else if (newMapsets.Count > 0)
            {
                var map = newMapsets.Last().Maps.Last();
                Console.WriteLine(map.Artist + " " + map.Title);

                // Switch map and load audio for song and play it.
                Map.ChangeSelected(map);
            }

            Logger.LogSuccess("Successfully completed the conversion task. Stopping loader.", LogType.Runtime);
            GameBase.GameStateManager.RemoveState();
        }
    }
}
