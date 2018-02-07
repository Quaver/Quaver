using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Quaver.Config;
using Quaver.Logging;
using Steamworks;

namespace Quaver.Steam
{
    internal static class SteamAPIHelper
    {
        /// <summary>
        ///     The application Id for steam
        /// </summary>
        internal static uint ApplicationId { get; } = 480;

        /// <summary>
        ///     Returns if the  SteamAPI is currently intitialized
        /// </summary>
        internal static bool IsInitialized { get; private set; }

        /// <summary>
        ///     Initializes our Steam API Helper
        ///     This should only be initalized once at the start of the game, and 
        ///     when attempting to access online features
        /// </summary>
        internal static void Initialize()
        {
            // Attempt to initialize the Steam API
            IsInitialized = SteamAPI.Init();

            // If the Steam initalization has failed, we'll need to prompt the user to restart the game with Steam.
            if (!IsInitialized)
            {
                Logger.LogError("SteamAPI.Init() call has failed! Steam is not initialized!", LogType.Runtime);

                // Display a dialog to users to restart the game with steam
                PromptToOpenSteam();

                return;
            }

            // Check for the correct assembly.
            if (!Packsize.Test())
                throw new Exception("The incorrect Steamworks.NET assembly was loaded for this platform!");

            // Check if the correct dlls were loaded for this platform, since we're running the game under mono, we can
            // load Windows x86 dlls
            if (!DllCheck.Test())
                throw new Exception("The wrong .dlls were loaded for this platform!");

#if DEBUG
            Logger.LogSuccess($"[STEAM API HELPER] Logged into Steam as: {SteamFriends.GetPersonaName()} <{SteamUser.GetSteamID()}>", LogType.Runtime);
#endif
        }

        /// <summary>
        ///     This function prompts the user to open the game with steam. If they say yes, then the game will restart with steam 
        ///     accordingly.
        /// </summary>
        private static void PromptToOpenSteam()
        {
            if (IsInitialized)
                return;
            
            // TODO: Add an actual prompt for this.
            Console.WriteLine("Open the game with steam?");
            var choice = Console.ReadLine();

            if (choice.ToLower() == "y" || choice.ToLower() == "yes")
                OpenWithSteam();
        }

        /// <summary>
        ///     Opens the game with steam. If in DEBUG mode, it will create a steam_appid.txt file with our application id
        ///     so that we can actually begin to use steam features in development.
        /// </summary>
        private static void OpenWithSteam()
        {
            // Makes the game start with steam. 
            // NOTE: During development, if the steam_appid.txt file is present, then it will return false,
            // meaning that we don't need to continuously launch with steam during development.
            if (!SteamAPI.RestartAppIfNecessary((AppId_t)ApplicationId))
                return;

#if DEBUG
            // Creates a file with the Steam Application Id, this is required for our game.
            File.WriteAllText($"{Directory.GetCurrentDirectory()}/steam_appid.txt", ApplicationId.ToString());

            Console.WriteLine(
                "In order to test Steam features in debug mode, you'll need to have a steam_appid.txt file. " +
                "One has already been created for you, and the program will now restart");
#endif
            // Quit game
            QuaverGame.Quit();
        }
    }
}
