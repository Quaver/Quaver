using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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

            // After initializing the config, we'll run an async task to initialize the beatmap database
            var dbTask = Task.Run(async () => await DatabaseHelper.InitializeBeatmapDatabaseAsync());

            // Wait for all relevant tasks to complete before starting the game.
            Task.WaitAll(dbTask);

            // Run all test methods as a task in the background - Game can be started during this time however.
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
        [Conditional("DEBUG")]
        private static void RunTestMethods()
        {
            Console.WriteLine("\n[DEBUG] Running Test Methods...\n");
            Task.Run(() => QuaTest.ParseQuaTest(true));
            Task.Run(() => SkinTest.ParseSkinTest(true));
        }
    }
}