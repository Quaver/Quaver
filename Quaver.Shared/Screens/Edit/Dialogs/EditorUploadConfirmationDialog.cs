using System;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorUploadConfirmationDialog : YesNoDialog
    {
        public EditorUploadConfirmationDialog(EditScreen screen) : base("UPLOAD MAPSET",
            "Are you sure you would like to upload your mapset to the server?\nNote: You must own the rights to all files you upload.")
        {
            YesAction += () =>
            {
                if (!IsMapsetEligibleToUpload(screen.Map))
                    return;

                DialogManager.Show(new EditorUploadingMapsetDialog(screen));
            };
        }

        public static bool IsMapsetEligibleToUpload(Map map)
        {
            var mapset = map.Mapset;

            if (mapset.Maps.Any(x => x.RankedStatus == RankedStatus.Ranked))
            {
                NotificationManager.Show(NotificationLevel.Warning, "You cannot submit a mapset that is already ranked!");
                return false;
            }

            if (mapset.Maps.Any(x => x.Creator != OnlineManager.Self?.OnlineUser?.Username))
            {
                NotificationManager.Show(NotificationLevel.Warning, "This mapset does not belong to you! " +
                                                                    "The creator usernames of every map do not match yours.");
                return false;
            }


            return true;
        }
    }
}