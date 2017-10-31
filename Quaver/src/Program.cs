using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Quaver.Audio;
using Quaver.Beatmaps;
using Quaver.Config;
using Quaver.Database;
using Quaver.Main;
using Quaver.Online.Patcher;
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
            // TODO: Test patching of files.
            //Patcher.PatchFiles(CpuFlag.Win64);

            /*
             * Display Resources Names
             * foreach (var file in System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames())
                Console.WriteLine(file);*/

            // Initialize Config
            Configuration.InitializeConfig();
 
            // Delete all the temporary data files.
            foreach (var file in new DirectoryInfo(Configuration.DataDirectory).GetFiles())
                file.Delete();

            // After initializing the configuration, we want to sync the beatmap database, and load the dictionary of beatmaps.
            var loadGame = Task.Run(async () =>
            {
                await GameBase.LoadAndSetBeatmaps();
                
                // The visible beatmaps in song select should be every single mapset at the start of the game.
                GameBase.VisibleBeatmaps = GameBase.Beatmaps;

                // Test Search
                //GameBase.VisibleBeatmaps = BeatmapUtils.SearchBeatmaps(GameBase.Beatmaps, "Camellia");
            });

            Task.WaitAll(loadGame);

            // Run all test methods
            Task.Run(() => RunTestMethods());

            // Start game
            using (var game = new QuaverGame())
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