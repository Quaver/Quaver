using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Quaver.Config;
using Quaver.Logging;
using Steamworks;

namespace Quaver.Steam
{
    internal class SteamAPIHelper
    {
        /// <summary>
        ///     The application Id for steam
        /// </summary>
        internal uint ApplicationId { get; } = 480;

        /// <summary>
        ///     Returns if the  SteamAPI is currently intitialized
        /// </summary>
        internal bool IsInitialized { get; }

        /// <summary>
        ///     Initializes our Steam API Helper
        /// </summary>
        internal SteamAPIHelper()
        {
            try
            {
#if DEBUG
                // Creates a file with the Steam Application Id, this is required for our game.
                File.WriteAllText($"{Directory.GetCurrentDirectory()}/steam_appid.txt", ApplicationId.ToString());
#endif
                // Makes the game start with steam. 
                // NOTE: During development, if the steam_appid.txt file is present, then it will return false,
                // meaning that we don't need to continuously launch with steam during development.
                if (SteamAPI.RestartAppIfNecessary((AppId_t)ApplicationId))
                {
                    Application.Exit();
                    return;
                }
            }
            catch (DllNotFoundException e)
            {
                var log = "[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib.";

                Logger.Log(log, LogColors.GameError);
                throw new DllNotFoundException(log);
            }

            // Attempt to initialize the Steam API
            IsInitialized = SteamAPI.Init();

            if (!IsInitialized)
            {
#if DEBUG
                var log = "SteamAPI.Init() call has failed! Steam has to be loaded in order for this to work!";
#else
                var log = "Call to SteamAPI.Init() has failed!";
#endif

                Logger.Log(log, LogColors.GameError);
                throw new Exception(log);
            }

            // Check for the correct assembly.
            if (!Packsize.Test())
            {
                var log = "The incorrect Steamworks.NET assembly was loaded for this platform!";


                Logger.Log(log, LogColors.GameError);
                throw new Exception(log);
            }

            // Check if the correct dlls were loaded for this platform, since we're running the game under mono, we can
            // load Windows x86 dlls
            if (!DllCheck.Test())
            {
                var log = "The wrong .dlls were loaded for this platform!";

                Logger.Log(log, LogColors.GameError);
                throw new Exception(log);
            }

#if DEBUG
            Console.WriteLine($"[STEAM API HELPER] Logged into Steam as: {SteamFriends.GetPersonaName()} <{SteamUser.GetSteamID()}>");
#endif

        }
    }
}
