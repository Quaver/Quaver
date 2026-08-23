using System;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorUploadConfirmationDialog : YesNoDialog
    {
        public EditorUploadConfirmationDialog(EditScreen screen) : base(
            LocalizationManager.Get("Screen_Editor_UploadMapset"),
            LocalizationManager.Get("Screen_Editor_UploadMapsetConfirmation"))
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
                NotificationManager.Show(NotificationLevel.Warning,
                    LocalizationManager.Get("Screen_Editor_CannotSubmitRankedMapset"));
                return false;
            }

            if (mapset.Maps.Any(x => x.Creator != OnlineManager.Self?.OnlineUser?.Username))
            {
                NotificationManager.Show(NotificationLevel.Warning,
                    LocalizationManager.Get("Screen_Editor_MapsetDoesNotBelongToYou"));
                return false;
            }


            return true;
        }
    }
}
