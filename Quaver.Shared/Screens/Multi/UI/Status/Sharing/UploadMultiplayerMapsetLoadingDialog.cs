using System;
using System.IO;
using Quaver.API.Helpers;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Multi.UI.Status.Sharing
{
    public class UploadMultiplayerMapsetLoadingDialog : LoadingDialog
    {
        public UploadMultiplayerMapsetLoadingDialog() : base("UPLOADING UNSUBMITTED MAPSET",
            "Please wait while the mapset is being uploaded...", () =>
            {
                try
                {
                    var gameId = OnlineManager.CurrentGame?.GameId ?? -1;
                    
                    
                    var path = MapManager.Selected.Value.Mapset.ExportToZip(false);

                    var packageMd5 = CryptoHelper.FileToMd5(path);
                    var mapMd5 = MapManager.Selected.Value.Md5Checksum;
                    
                    var success = OnlineManager.Client.UploadSharedMultiplayerMapset(path, gameId, mapMd5, packageMd5);

                    if (!success)
                        throw new Exception("Failure sharing multiplayer mapset");

                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, LogType.Runtime);
                    }

                    var log = $"Successfully uploaded unsubmitted mapset for the multiplayer game!";
                    NotificationManager.Show(NotificationLevel.Success, log);
                }
                catch (Exception e)
                {
                    NotificationManager.Show(NotificationLevel.Error, "There was an error while uploading the mapset.");
                    Logger.Error(e, LogType.Network);
                }
            })
        {
        }
    }
}