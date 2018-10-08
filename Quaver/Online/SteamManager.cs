using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using Wobble;
using Wobble.Logging;

namespace Quaver.Online
{
    public static class SteamManager
    {
        /// <summary>
        ///     The application id for steam.
        /// </summary>
        public static uint ApplicationId => 480;

        /// <summary>
        ///     Determines if steam is initialized or not.
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        ///     The Steam auth session ticket handle
        /// </summary>
        public static HAuthTicket AuthSessionTicket { get; private set; }

        /// <summary>
        ///     The buffer that contains the actual session ticket
        /// </summary>
        public static byte[] PTicket { get; set; }

        /// <summary>
        ///     PCB Ticket
        /// </summary>
        public static uint PcbTicket;

        /// <summary>
        ///     Determines if the auth session ticket we have is validated.
        /// </summary>
        public static bool AuthSessionTicketValidated { get; private set; }

        /// <summary>
        ///     The user's steam avatar.
        /// </summary>
        public static Texture2D UserAvatar { get; set; }

        #region Callbacks

        /// <summary>
        ///     The callback that will be ran when the client requests for an auth session ticket
        /// </summary>
        private static Callback<GetAuthSessionTicketResponse_t> GetAuthSessionTickResponse { get; set; }
#endregion

        /// <summary>
        ///     Initializes the Steam API.
        /// </summary>
        public static void Initialize()
        {
#if DEBUG
            // Creates a file with the Steam Application Id, this is required for debugging
            File.WriteAllText($"{Directory.GetCurrentDirectory()}/steam_appid.txt", ApplicationId.ToString());
#endif
            // Make sure the game is started with Steam.
            if (SteamAPI.RestartAppIfNecessary((AppId_t) ApplicationId))
            {
                Environment.Exit(0);
                return;
            }

            IsInitialized = SteamAPI.Init();

            if (!IsInitialized)
            {
                Logger.Error($"SteamAPI.Init() call has failed! Steam is not initialized", LogType.Network);
                throw new Exception();
            }

            if (!Packsize.Test())
            {
                Logger.Error($"The incorrect Steamworks.NET assembly was loaded for this platform!", LogType.Runtime);
                throw new Exception();
            }

            if (!DllCheck.Test())
            {
                Logger.Error($"The wrong dlls were loaded for this platform!", LogType.Runtime);
                throw new Exception();
            }

            Logger.Important($"Successfully initialized and logged into Steam as : {SteamFriends.GetPersonaName()} " +
                              $"<{SteamUser.GetSteamID()}>", LogType.Runtime);

            InitializeCallbacks();
            StartAuthSession();
        }

        /// <summary>
        ///     Initializes the important callbacks that we'll need from steam.
        /// </summary>
        private static void InitializeCallbacks()
        {
            GetAuthSessionTickResponse = Callback<GetAuthSessionTicketResponse_t>.Create(OnValidateAuthSessionTicketResponse);
        }

        /// <summary>
        ///     Starts the authentication session so that we can log into the Quaver server afterwards.
        /// </summary>
        private static void StartAuthSession()
        {
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
                    AuthSessionTicketValidated = true;
                    break;
                // All error cases returned from Steam
                default:
                    Logger.Error("Could not generate an auth session ticket!", LogType.Runtime);
                    return;
            }
        }

        /// <summary>
        ///     Gets a small steam avatar.
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public static Texture2D GetAvatar(ulong steamId)
        {
            var avatar = SteamFriends.GetLargeFriendAvatar(new CSteamID(steamId));

            Texture2D ret = null;

            var bIsValid = SteamUtils.GetImageSize(avatar, out var imageWidth, out var imageHeight);

            if (!bIsValid)
                return null;

            var image = new byte[imageWidth * imageHeight * 4];

            bIsValid = SteamUtils.GetImageRGBA(avatar, image, (int)(imageWidth * imageHeight * 4));

            if (!bIsValid)
                return null;

            ret = new Texture2D(GameBase.Game.GraphicsDevice, (int)imageWidth, (int)imageHeight);

            ret.SetData(image);
            return ret;
        }
    }
}