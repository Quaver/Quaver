/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Steamworks;
using Wobble;
using Wobble.Logging;
using Wobble.Platform.Linux;

namespace Quaver.Shared.Online
{
    public static class SteamManager
    {
        /// <summary>
        ///     The application id for steam.
        /// </summary>
        public static uint ApplicationId => 980610;

        /// <summary>
        ///     Determines if steam is initialized or not.
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        ///     The avatars for steam users.
        /// </summary>
        public static ConcurrentDictionary<ulong, Texture2D> UserAvatars { get; private set; }

        /// <summary>
        ///     Large Steam user avatars
        /// </summary>
        public static ConcurrentDictionary<ulong, Texture2D> UserAvatarsLarge { get; private set; }

        /// <summary>
        ///     A user's steam avatar has loaded.
        /// </summary>
        public static EventHandler<SteamAvatarLoadedEventArgs> SteamUserAvatarLoaded;

        /// <summary>
        ///     The ids of the skins that are available in the workshop
        /// </summary>
        public static List<PublishedFileId_t> WorkshopItemIds { get; private set; } = new List<PublishedFileId_t>();

        #region Callbacks

        /// <summary>
        ///     The callback that will be ran when the client requests for an auth session ticket
        /// </summary>
        private static Callback<GetAuthSessionTicketResponse_t> GetAuthSessionTickResponse { get; set; }

        /// <summary>
        ///     Called when receiving a persona state change from steam (for user avatars)
        /// </summary>
        private static Callback<PersonaStateChange_t> PersonaStateChanged { get; set; }

        /// <summary>
        ///     Called when submitting the workshop update has completed
        /// </summary>
        /// <returns></returns>
        public static CallResult<SubmitItemUpdateResult_t> OnSubmitUpdateResponse { get; private set; }

        /// <summary>
        ///     Called after calling ISteamUGC::CreateItem.
        /// </summary>
        public static CallResult<CreateItemResult_t> OnCreateItemResponse { get; private set; }

        /// <summary>
        ///     Called after subscribing to a workshop item
        /// </summary>
        public static CallResult<RemoteStoragePublishedFileSubscribed_t> OnFileSubscribedResponse { get; private set; }

        /// <summary>
        ///     Called after unsubscribing to a workshop item
        /// </summary>
        public static CallResult<RemoteStoragePublishedFileUnsubscribed_t> OnFileUbsubscribedResponse { get; private set; }

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

            UserAvatars = new ConcurrentDictionary<ulong, Texture2D>();
            UserAvatarsLarge = new ConcurrentDictionary<ulong, Texture2D>();

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

            // Prevents a crash on OSX for the time being
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                RefreshWorkshopSkins();

            // Set the rich presence.
            // note that this depends on the localization file.

            // have am initial value for State and details, so it doesn't just show "%state%: %details%" on Beta warning screen.
            SteamFriends.SetRichPresence("State", "Loading");
            SteamFriends.SetRichPresence("Details", "Launching the game");
            // set our "displayed key" to #Status.  
            SteamFriends.SetRichPresence("steam_display", "#Status");


            // DANGEROUS: Uncomment to reset all achievements
            // SteamUserStats.ResetAllStats(true);
        }

        /// <summary>
        ///     Initializes the important callbacks that we'll need from steam.
        /// </summary>
        private static void InitializeCallbacks()
        {
            PersonaStateChanged = Callback<PersonaStateChange_t>.Create(OnPersonaStateChanged);
            OnSubmitUpdateResponse = CallResult<SubmitItemUpdateResult_t>.Create(OnSubmittedItemUpdate);
            OnCreateItemResponse = CallResult<CreateItemResult_t>.Create(OnCreateItemResultCallResponse);
            OnFileSubscribedResponse = CallResult<RemoteStoragePublishedFileSubscribed_t>.Create(OnFileSubscribed);
            OnFileUbsubscribedResponse = CallResult<RemoteStoragePublishedFileUnsubscribed_t>.Create(OnfileUnsubscribed);
        }

        /// <summary>
        ///     Requests to steam to retrieve a user's avatar.
        /// </summary>
        public static void SendAvatarRetrievalRequest(ulong steamId) => ThreadScheduler.Run(() =>
        {
            var info = SteamFriends.RequestUserInformation(new CSteamID(steamId), false);
            Logger.Debug($"Requested Steam user information for user: {steamId} - {info}", LogType.Network);

            if (!info)
                LoadAvatarIfNotExists(steamId);
        });

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
        ///     Called after submitting an update
        /// </summary>
        /// <param name="result"></param>
        /// <param name="bIOfailure"></param>
        public static void OnSubmittedItemUpdate(SubmitItemUpdateResult_t result, bool bIOfailure)
        {
            SteamWorkshopItem.Current.HasUploaded = true;

            if (bIOfailure)
            {
                Logger.Error("Failed to Create workshop item:\n" +
                             $"m_eResult: {result.m_eResult}\n" +
                             $"m_nPublishedFileId: {result.m_nPublishedFileId}\n" +
                             $"m_bUserNeedsToAcceptWorkshopLegalAgreement: {result.m_bUserNeedsToAcceptWorkshopLegalAgreement}", LogType.Network);

                return;
            }

            Logger.Important($"Workshop upload result: {result.m_eResult}", LogType.Network);

            if (result.m_eResult == EResult.k_EResultOK)
            {
                Logger.Important($"Workshop upload successful!", LogType.Network);
                NotificationManager.Show(NotificationLevel.Success, "You have successfully uploaded your Steam workshop skin!");
                BrowserHelper.OpenURL($"https://steamcommunity.com/sharedfiles/filedetails/?id={result.m_nPublishedFileId.m_PublishedFileId}");
            }
            else
            {
                Logger.Important($"Workshop upload failed! - {result.m_eResult}", LogType.Network);

                string text;
                if (result.m_eResult == EResult.k_EResultLimitExceeded)
                    text = "Workshop upload failed. Check that your steam_preview_image.png is below 1 MB and that you have enough free space on the Steam Cloud.";
                else
                    text = $"The Steam workshop upload has failed due to: {result.m_eResult}";

                NotificationManager.Show(NotificationLevel.Error, text);
            }
        }

        /// <summary>
        ///     Called when after calling SteamUGC.CreateItem();
        /// </summary>
        /// <param name="result"></param>
        /// <param name="bIOfailure"></param>
        public static void OnCreateItemResultCallResponse(CreateItemResult_t result, bool bIOfailure)
        {
            if (bIOfailure)
            {
                Logger.Error("Failed to Create workshop item:\n" +
                             $"m_eResult: {result.m_eResult}\n" +
                             $"m_nPublishedFileId: {result.m_nPublishedFileId}\n" +
                             $"m_bUserNeedsToAcceptWorkshopLegalAgreement: {result.m_bUserNeedsToAcceptWorkshopLegalAgreement}", LogType.Network);

                SteamWorkshopItem.Current.HasUploaded = true;
                return;
            }

            Logger.Important($"Starting Steam workshop upload....", LogType.Runtime);

            // Open in Steam client to accept legal agreement for workshop
            if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
            {
                BrowserHelper.OpenURL($"steam://url/CommunityFilePage/{result.m_nPublishedFileId}");
                SteamWorkshopItem.Current.HasUploaded = true;
                return;
            }

            var publishedFileId = result.m_nPublishedFileId;

            SteamWorkshopItem.Current.Handle = SteamUGC.StartItemUpdate((AppId_t) ApplicationId, publishedFileId);

            // Write a file with the workshop id
            File.WriteAllText(SteamWorkshopItem.Current.WorkshopIdFilePath, result.m_nPublishedFileId.m_PublishedFileId.ToString());

            if (SteamWorkshopItem.Current.ExistingWorkshopFileId == 0)
            {
                SteamUGC.SetItemTitle(SteamWorkshopItem.Current.Handle, SteamWorkshopItem.Current.Title);
                SteamUGC.SetItemVisibility(SteamWorkshopItem.Current.Handle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate);
            }

            if (SteamWorkshopItem.Current.PreviewFilePath != null && File.Exists(SteamWorkshopItem.Current.PreviewFilePath))
                SteamUGC.SetItemPreview(SteamWorkshopItem.Current.Handle, SteamWorkshopItem.Current.PreviewFilePath);

            SteamUGC.SetItemContent(SteamWorkshopItem.Current.Handle, SteamWorkshopItem.Current.FolderPath);
            
            var tagUpdate = false; 
            
            if (File.Exists($"{SteamWorkshopItem.Current.FolderPath}/skin.ini"))
                tagUpdate = SteamUGC.SetItemTags(SteamWorkshopItem.Current.Handle, new List<string> {"Skins"});
            else if (File.Exists($"{SteamWorkshopItem.Current.FolderPath}/settings.ini"))
                tagUpdate = SteamUGC.SetItemTags(SteamWorkshopItem.Current.Handle, new List<string> {"Plugins"});

            if (!tagUpdate)
                NotificationManager.Show(NotificationLevel.Error, $"Failed to update tags for the workshop item. Please report this!");

            var call = SteamUGC.SubmitItemUpdate(SteamWorkshopItem.Current.Handle, "");
            OnSubmitUpdateResponse.Set(call);
        }

        /// <summary>
        ///     Sets a Rich Presence key/value for the current user that is automatically shared to all friends playing the same game.
        /// </summary>
        /// <remarks>https://partner.steamgames.com/doc/api/ISteamFriends#SetRichPresence</remarks>
        /// <param name="pchKey">The rich presence 'key' to set. This can not be longer than specified in k_cchMaxRichPresenceKeyLength.</param>
        /// <param name="pchValue">The rich presence 'value' to associate with pchKey. This can not be longer than specified in k_cchMaxRichPresenceValueLength.
        /// If this is set to an empty string ("") or NULL then the key is removed if it's set.</param>
        /// <returns>boolean
        /// true - if the rich presence was set successfully.
        /// false - if failed.
        /// </returns>
        public static bool SetRichPresence(string pchKey, string pchValue)
        {
            var result = SteamFriends.SetRichPresence(pchKey, pchValue);
            if (!result)
            {
                Logger.Error($"setting rich presence key ${pchKey} failed, please check the Rich Presence Tester for more details.", LogType.Runtime);
            }

            return result;
        }

        /// <summary>
        ///     Clears all of the current user's Rich Presence key/values.
        /// </summary>
        public static void ClearRichPresence()
        {
            SteamFriends.ClearRichPresence();
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
            UserAvatarsLarge[steamId] = LoadAvatar(steamId, true) ?? UserInterface.UnknownAvatar;

            SteamUserAvatarLoaded?.Invoke(typeof(SteamManager), new SteamAvatarLoadedEventArgs(steamId, tex));

            Logger.Debug($"Loaded Steam Avatar for user: {steamId}", LogType.Network);
        }

        /// <summary>
        ///     Loads a user's avatar from steam
        /// </summary>
        /// <returns></returns>
        private static Texture2D LoadAvatar(ulong steamId, bool large = false)
        {
            // Get the icon type as a integer.
            var icon = large ? SteamFriends.GetLargeFriendAvatar(new CSteamID(steamId)) : SteamFriends.GetMediumFriendAvatar(new CSteamID(steamId));

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

        /// <summary>
        ///     Gets a user's avatar from <see cref="UserAvatars"/> or returns <see cref="UserInterface.UnknownAvatar"/> if it doesn't exist.
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public static Texture2D GetAvatarOrUnknown(ulong steamId)
        {
            if (UserAvatars.ContainsKey(steamId))
                return UserAvatars[steamId];

            return UserInterface.UnknownAvatar;
        }

        /// <summary>
        /// </summary>
        public static void RefreshWorkshopSkins()
        {
            try
            {
                var numSubscribed = SteamUGC.GetNumSubscribedItems();

                PublishedFileId_t[] fileIds = {};
                var entries = SteamUGC.GetSubscribedItems(fileIds, 1);

                Logger.Important($"Found {fileIds.Length} subscribed workshop items | # of subscribed: " +
                                 $"{numSubscribed} | Entries: {entries}", LogType.Runtime);

                WorkshopItemIds = new List<PublishedFileId_t>(fileIds);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="result"></param>
        /// <param name="biofailure"></param>
        private static void OnFileSubscribed(RemoteStoragePublishedFileSubscribed_t result, bool biofailure)
        {
            if (result.m_nAppID.m_AppId != ApplicationId)
                return;

            if (WorkshopItemIds.Any(x => x.m_PublishedFileId == result.m_nPublishedFileId.m_PublishedFileId))
                return;

            Logger.Important($"Subscribed to fule: {result.m_nPublishedFileId}", LogType.Runtime);
            RefreshWorkshopSkins();
        }

        /// <summary>
        /// </summary>
        /// <param name="result"></param>
        /// <param name="biofailure"></param>
        private static void OnfileUnsubscribed(RemoteStoragePublishedFileUnsubscribed_t result, bool biofailure)
        {
            if (result.m_nAppID.m_AppId != ApplicationId)
                return;

            Logger.Important($"Unsubscribed from file: {result.m_nPublishedFileId}", LogType.Runtime);
            RefreshWorkshopSkins();
        }
    }
}
