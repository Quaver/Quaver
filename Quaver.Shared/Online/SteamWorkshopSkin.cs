using System;
using System.IO;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Steamworks;
using Wobble.Logging;

namespace Quaver.Shared.Online
{
    public class SteamWorkshopSkin
    {
        /// <summary>
        ///     The current workshop skin upload
        /// </summary>
        public static SteamWorkshopSkin Current { get; private set; }

        /// <summary>
        ///     The handle used to start making calls to upload the skin
        /// </summary>
        public UGCUpdateHandle_t Handle { get; set; }

        /// <summary>
        ///     The title of the skin
        /// </summary>
        public string Title { get; }

        /// <summary>
        ///     The path to the file
        /// </summary>
        public string PreviewFilePath { get; }

        /// <summary>
        ///     The path to the skin folder
        /// </summary>
        public string SkinFolderPath { get; }

        /// <summary>
        ///     The file path of the text file that contains the id of the skin
        /// </summary>
        public string WorkshopIdFilePath => $"{SkinFolderPath}/steam_workshop_id.txt";

        /// <summary>
        ///     The id of the existing workshop file (if updating)
        /// </summary>
        public ulong ExistingWorkshopFileId { get; private set; }

        /// <summary>
        ///     If the skin has uploaded
        /// </summary>
        public bool HasUploaded { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="skin"></param>
        public SteamWorkshopSkin(string skin)
        {
            if (Current != null && !Current.HasUploaded)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You must wait until the previous upload has completed!");
                return;
            }

            Current = this;
            Title = skin;
            SkinFolderPath = $"{ConfigManager.SkinDirectory.Value}/{skin}".Replace("\\", "/");
            PreviewFilePath = $"{SkinFolderPath}/steam_workshop_preview.png";

            if (!File.Exists(PreviewFilePath))
            {
                NotificationManager.Show(NotificationLevel.Error,
                    "You must place a steam_workshop_preview.png in the skin folder in order to upload it.");

                HasUploaded = true;
                return;
            }
        }

        /// <summary>
        /// </summary>
        public void Upload()
        {
            if (File.Exists(WorkshopIdFilePath))
            {
                ExistingWorkshopFileId = ulong.Parse(File.ReadAllText(WorkshopIdFilePath));

                var result = new CreateItemResult_t
                {
                    m_bUserNeedsToAcceptWorkshopLegalAgreement = false,
                    m_eResult = EResult.k_EResultOK,
                    m_nPublishedFileId = new PublishedFileId_t(ExistingWorkshopFileId)
                };

                SteamManager.OnCreateItemResultCallResponse(result, false);
                return;
            }

            var resp = SteamUGC.CreateItem((AppId_t) SteamManager.ApplicationId, EWorkshopFileType.k_EWorkshopFileTypeCommunity);
            SteamManager.OnCreateItemResponse.Set(resp);
        }

        /// <summary>
        ///     Returns the upload progress percentage
        /// </summary>
        /// <returns></returns>
        public int GetUploadProgressPercentage()
        {
            SteamUGC.GetItemUpdateProgress(Handle, out var bytesProcessed, out var bytesTotal);
            return (int) (bytesProcessed / bytesTotal * 100);
        }
    }
}