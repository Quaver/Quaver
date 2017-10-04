using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Quaver.QuaFile;
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
            // Initialize quaver.cfg
            var configTask = Task.Factory.StartNew(() =>
            {
                Console.WriteLine("[MAIN] Initializing Config...");
                Configuration.InitializeConfig();
            });

            // Initialize beatmap database.
            var beatmapDatabaseTask = Task.Factory.StartNew(async () =>
            {
                Console.WriteLine("[MAIN] Initializing/Reading Beatmap Database...");
                await DatabaseHelper.InitializeBeatmapDatabaseAsync();
            });
            
            // Run Test Methods
            Task.Run(() =>
            {
                Console.WriteLine("[DEBUG] Running Test Methods...");
                RunTestMethods();
            });

            //Wait for all of the setup tasks to complete before running the game.
            Task.WaitAll(configTask, beatmapDatabaseTask);

            // Start game
            using (var game = new Game1())
            {
                game.Run();
            }
        }

        /// <summary>
        /// This'll run all of the test methods in our code. 
        /// They should be marked with [Conditional("DEBUG")]
        /// These will only be ran when the solution was built in debug mode.
        /// All functions in this method should have an argument "run",
        /// which specifies whether or not you want to run the specific method
        /// </summary>
        [Conditional("DEBUG")]
        private static void RunTestMethods()
        {
            Task.Run(() => QuaTest.ParseQuaTest(true));
            Task.Run(() => SkinTest.ParseSkinTest(true));
        }
    }
}