/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Steamworks;
using Wobble;
using Wobble.Logging;

namespace Quaver.Shared.Online
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
        ///     The avatars for steam users.
        /// </summary>
        public static Dictionary<ulong, Texture2D> UserAvatars { get; set; }

        /// <summary>
        ///     A user's steam avatar has loaded.
        /// </summary>
        public static EventHandler<SteamAvatarLoadedEventArgs> SteamUserAvatarLoaded;

        #region Callbacks

        /// <summary>
        ///     The callback that will be ran when the client requests for an auth session ticket
        /// </summary>
        private static Callback<GetAuthSessionTicketResponse_t> GetAuthSessionTickResponse { get; set; }

        /// <summary>
        ///     Called when receiving a persona state change from steam (for user avatars)
        /// </summary>
        private static Callback<PersonaStateChange_t> PersonaStateChanged { get; set; }

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
                Environment.Exit(0);

            IsInitialized = SteamAPI.Init();

            UserAvatars = new Dictionary<ulong, Texture2D>();

            if (!IsInitialized)
            {
                var log = $"SteamAPI.Init() call has failed! Steam is not initialized";
                Logger.Error(log, LogType.Network);
                throw new InvalidOperationException(log);
            }

            if (!Packsize.Test())
            {
                var log = $"The incorrect Steamworks.NET assembly was loaded for this platform!";
                Logger.Error(log, LogType.Runtime);
                throw new InvalidOperationException(log);
            }

            if (!DllCheck.Test())
            {
                var log = "The wrong dlls were loaded for this platform!";
                Logger.Error(log, LogType.Runtime);
                throw new InvalidOperationException(log);
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
            PersonaStateChanged = Callback<PersonaStateChange_t>.Create(OnPersonaStateChanged);
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
        ///     Requests to steam to retrieve a user's avatar.
        /// </summary>
        public static void SendAvatarRetrievalRequest(ulong steamId)
        {
            var info = SteamFriends.RequestUserInformation(new CSteamID(steamId), false);
            Logger.Debug($"Requested Steam user information for user: {steamId} - {info}", LogType.Network);

            if (!info)
                LoadAvatarIfNotExists(steamId);
        }

        /// <summary>
        ///     Called when a requested user's persona state has changed.
        /// </summary>
        /// <param name="callback"></param>
        private static void OnPersonaStateChanged(PersonaStateChange_t callback)
        {
            if (callback.m_nChangeFlags == EPersonaChange.k_EPersonaChangeAvatar)
                LoadAvatarIfNotExists(callback.m_ulSteamID);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="steamId"></param>
        private static void LoadAvatarIfNotExists(ulong steamId)
        {
            Logger.Debug($"Requesting Steam Avatar for user: {steamId}...", LogType.Network);

            var tex = LoadAvatar(steamId) ?? UserInterface.UnknownAvatar;

            UserAvatars[steamId] = tex;
            SteamUserAvatarLoaded?.Invoke(typeof(SteamManager), new SteamAvatarLoadedEventArgs(steamId, tex));

            Logger.Debug($"Loaded Steam Avatar for user: {steamId}", LogType.Network);
        }

        /// <summary>
        ///     Loads a user's avatar from steam
        /// </summary>
        /// <returns></returns>
        private static Texture2D LoadAvatar(ulong steamId)
        {
            // Get the icon type as a integer.
            var icon = SteamFriends.GetMediumFriendAvatar(new CSteamID(steamId));

            // Check if we got an icon type.
            if (icon != 0)
            {
                var ret = SteamUtils.GetImageSize(icon, out var width, out var height);

                if (ret && width > 0 && height > 0)
                {
                    var rgba = new byte[width * height * 4];
                    ret = SteamUtils.GetImageRGBA(icon, rgba, rgba.Length);
                    if (ret)
                    {
                        var texture = new Texture2D(GameBase.Game.GraphicsDevice, (int)width, (int)height, false, SurfaceFormat.Color);
                        texture.SetData(rgba, 0, rgba.Length);
                        return texture;
                    }
                }
            }

            return null;
        }
    }
}
