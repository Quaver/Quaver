using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Quaver.Audio;
using Quaver.Beatmaps;
using Quaver.Config;
using Quaver.Database;
using Quaver.Tests;

namespace Quaver
{
    /// <summary>
    ///     The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Initialize Config
            Configuration.InitializeConfig();

            // Run all test methods as a task in the background - Game can be started during this time however.
            Task.Run(() => RunTestMethods());

            // After initializing the config, we'll run an async task to initialize the beatmap database,
            // then proceed to load all of the beatmaps in the database.
            List<Beatmap> beatmapList = new List<Beatmap>();
            var dbTask = Task.Run(async () => beatmapList = await BeatmapCache.LoadBeatmapDatabaseAsync());

            // Wait for all relevant tasks to complete before starting the game.
            Task.WaitAll(dbTask);

            // Sort all the beatmaps by artist -> then title.
            Dictionary<string, List<Beatmap>> beatmaps = BeatmapUtils.OrderBeatmapsByArtist(BeatmapUtils.GroupBeatmapsByDirectory(beatmapList));
            Console.WriteLine($"[GAME] Successfully loaded {beatmapList.Count} in {beatmaps.Keys.Count} directories.");

            // Start watching for directory changes.
            Task.Run(() => BeatmapImporter.WatchForChanges());

            // Start game
            using (var game = new Game1())
            {
                game.Run();
            }
        }

        /// <summary>
        ///     This'll run all of the test methods in our code.
        ///     They should be marked with [Conditional("DEBUG")]
        ///     These will only be ran when the solution was built in debug mode.
        ///     All functions in this method should have an argument "run",
        ///     which specifies whether or not you want to run the specific method
        /// </summary>
        private static void RunTestMethods()
        {
            Console.WriteLine("\n[DEBUG] Running Test Methods if there are any...\n");
            Task.Run(() => QuaTest.ParseQuaTest(false));
            Task.Run(() => SkinTest.ParseSkinTest(false));
            Task.Run(() => AudioTest.PlaySongPreview(true));
        }
    }
}