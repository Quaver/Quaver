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
using Quaver.Commands;
using Quaver.Config;
using Quaver.Database;
using Quaver.Discord;
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

            // Initialize Discord RichPresence
            InitializeDiscordPresence();

            // Delete Temp Files
            DeleteTemporaryFiles();
 
            // Set up the game
            SetupGame();
      
            // Start game
            using (var game = new QuaverGame())
            {
                game.Run();
            }
        }

        /// <summary>
        ///     Responsible for initializing the Discord Presence
        /// </summary>
        private static void InitializeDiscordPresence()
        {
            if (GameBase.DiscordController != null)
                return;

            GameBase.DiscordController = new DiscordController();
            GameBase.DiscordController.Initialize();

            // Create a new RichPresence
            GameBase.DiscordController.presence = new DiscordRPC.RichPresence()
            {
                details = "Idle",
                largeImageKey = "quaver",
                largeImageText = Configuration.Username
            };
            DiscordRPC.UpdatePresence(ref GameBase.DiscordController.presence);
        }

        /// <summary>
        ///     Deletes all temporary files if there are any.
        /// </summary>
        private static void DeleteTemporaryFiles()
        {
            foreach (var file in new DirectoryInfo(Configuration.DataDirectory).GetFiles())
                file.Delete();
        }

        /// <summary>
        ///     Responsible for initializing and setting the beatmap database and setting the loaded beatmaps
        /// </summary>
        private static void SetupGame()
        {
            // After initializing the configuration, we want to sync the beatmap database, and load the dictionary of beatmaps.
            var loadGame = Task.Run(async () =>
            {
                await GameBase.LoadAndSetBeatmaps();

                // The visible beatmaps in song select should be every single mapset at the start of the game.
                GameBase.VisibleBeatmaps = GameBase.Beatmaps;
            });

            Task.WaitAll(loadGame);
        }
    }
}