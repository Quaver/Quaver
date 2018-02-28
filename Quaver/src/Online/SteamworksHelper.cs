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
using Quaver.Net;
using Steamworks;

namespace Quaver.Online
{
    internal static class SteamworksHelper
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
        ///     The Steam auth session ticket handle
        /// </summary>
        internal static HAuthTicket AuthSessionTicket { get; private set; }

        /// <summary>
        ///     The buffer that contains the actual session ticket
        /// </summary>
        internal static byte[] PTicket { get; set; }

        /// <summary>
        ///     PCB Ticket
        /// </summary>
        internal static uint PcbTicket;

        /// <summary>
        ///     Determines if we are connecting to the development server
        /// </summary>
        internal static bool ConnectingToDevServer { get; private set;  }

#region Callbacks 

        /// <summary>
        ///     The callback that will be ran when the client requests for an auth session ticket
        /// </summary>
        private static Callback<GetAuthSessionTicketResponse_t> GetAuthSessionTickResponse { get; set; }

#endregion

        /// <summary>
        ///     Initializes our Steam API Helper
        ///     This should only be initalized once at the start of the game, and 
        ///     when attempting to access online features
        /// </summary>
        internal static void Initialize()
        {
#if DEBUG
            // Creates a file with the Steam Application Id, this is required for debugging
            File.WriteAllText($"{Directory.GetCurrentDirectory()}/steam_appid.txt", ApplicationId.ToString());

            Console.WriteLine("In order to test Steam features in the debug release, you'll need to have a steam_appid.txt file." +
                            "One has already been created for you, so you can proceed as normal.");
#endif

            // Make sure the game is started with steam.
            if (SteamAPI.RestartAppIfNecessary((AppId_t) ApplicationId))
            {
                QuaverGame.Quit();
                return;
            }
                
            // Attempt to initialize the Steam API
            IsInitialized = SteamAPI.Init();

            // If the Steam initalization has failed, we'll need to prompt the user to restart the game with Steam.
            if (!IsInitialized)
            {
                Logger.LogError("SteamAPI.Init() call has failed! Steam is not initialized!", LogType.Runtime);
                QuaverGame.Quit();
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
            Logger.LogSuccess(
                $"[STEAM API HELPER] Logged into Steam as: {SteamFriends.GetPersonaName()} <{SteamUser.GetSteamID()}>",
                LogType.Runtime);
#endif

            // Initialize all Steam callbacks
            InitializeCallbacks();
            
            // Auto-connect to Flamingo
            ConnectToFlamingo();
        }

        /// <summary>
        ///     Initializes all of the required Steam callbacks for Quaver.
        /// </summary>
        private static void InitializeCallbacks()
        {
            GetAuthSessionTickResponse = Callback<GetAuthSessionTicketResponse_t>.Create(OnValidateAuthSessionTicketResponse);
        }

        /// <summary>
        ///     Connects to the Flamingo server
        /// </summary>
        private static void ConnectToFlamingo(bool devServer = false)
        {
            // Set if we're connecting to the development server
            if (devServer)
                ConnectingToDevServer = true;

            // If we're still not initialized at this point, we shouldn't continue
            if (!IsInitialized)
            {
                QuaverGame.Quit();
                return;
            }

            // Generate an auth session token and wait for a response from Steam
            // After calling this, it should call OnValidateAuthSessionTicketResponse(GetAuthSessionTicketResponse_t pCallback);
            // where we will then continue to authenticate the user
            PTicket = new byte[1024];
            AuthSessionTicket = SteamUser.GetAuthSessionTicket(PTicket, PTicket.Length, out PcbTicket);
        }

        /// <summary>
        ///     Called after attempting to generate an auth session ticket.
        ///     This further connects the user to the server
        /// </summary>
        /// <param name="pCallback"></param>
        private static void OnValidateAuthSessionTicketResponse(GetAuthSessionTicketResponse_t pCallback)
        {
            // Make the server login request if we've received confirmation that the auth session ticket
            // was successfully created
            switch (pCallback.m_eResult)
            {
                // Send the login request to Flamingo.
                case EResult.k_EResultOK:
                    Flamingo.Connect(SteamUser.GetSteamID().ToString(), SteamFriends.GetPersonaName(), PTicket, PcbTicket, ConnectingToDevServer);
                    break;
                // All error cases returned from Steam
                default:
                    Logger.LogError("Could not generate an auth session ticket!", LogType.Runtime);
                    return;
            }
        }
    }
}