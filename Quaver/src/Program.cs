using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Quaver.Audio;
using Quaver.Beatmaps;
using Quaver.Config;
using Quaver.Database;
using Quaver.Main;
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

            // After initializing the configuration, we want to sync the beatmap database, and load the dictionary of beatmaps.
            var loadGame = Task.Run(async () => await GameBase.LoadAndSetBeatmaps());
            Task.WaitAll(loadGame);

            // Start watching for beatmap directory changes.
            BeatmapImporter.WatchForChanges();

            // Run all test methods
            Task.Run(() => RunTestMethods());

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
            Console.WriteLine("\n[DEBUG] Running Test Methods if there are any...");
            Task.Run(() => QuaTest.ParseQuaTest(false));
            Task.Run(() => AudioTest.PlaySongPreview(false));
            Task.Run(() => JsonTest.DeserializeJsonTest(false));
        }
    }
}