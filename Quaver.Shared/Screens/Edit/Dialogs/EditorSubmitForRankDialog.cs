using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quaver.API.Maps;
using Quaver.API.Maps.AutoMod;
using Quaver.API.Maps.AutoMod.Issues;
using Quaver.Server.Client;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Edit.UI.AutoMods;
using Wobble.Managers;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorSubmitForRankDialog : LoadingDialog
    {
        public EditorSubmitForRankDialog(EditScreen screen) : base(
            LocalizationManager.Get("Screen_Editor_SubmitForRank"),
            LocalizationManager.Get("Screen_Editor_SubmittingForRankMessage"), () =>
        {
            // Run AutoMod and search for any issues.
            var autoMod = new AutoModMapset(screen.Map.Mapset.Maps.Select(x => x.LoadQua()).ToList());
            autoMod.Run();

            var mapsetIssues = autoMod.Issues.Any(x => x.Level > AutoModIssueLevel.Warning);
            var mapIssues = autoMod.Mods.Any(x => x.Value.Issues.Any(y => y.Level > AutoModIssueLevel.Warning));

            if (mapsetIssues || mapIssues)
            {
                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("Screen_Editor_RankingIssuesFound"));

                var view = screen.View as EditScreenView;

                if (view != null)
                {
                    view.AutoMod.Panel.RunAutoMod();
                    view.AutoMod.IsActive.Value = true;
                }

                return;
            }

            // Lastly, submit for rank.
            try
            {
                var response = OnlineManager.Client?.SubmitForRank(screen.Map.MapSetId);

                if (response == null)
                    throw new ArgumentNullException($"No response received from the server.");

                var responseParsed = (JToken)JsonConvert.DeserializeObject(response.Content);


                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var msg = responseParsed!["message"]?.ToString();
                        NotificationManager.Show(NotificationLevel.Success, msg);
                        Logger.Important($"Successfully submitted mapset for rank - {msg}", LogType.Network);
                        break;
                    default:
                        var err = responseParsed!["error"]?.ToString();
                        NotificationManager.Show(NotificationLevel.Error, err);
                        Logger.Important($"Failed to submit mapset for rank - {err}", LogType.Network);
                        break;
                }
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("Screen_Editor_SubmitForRankError"));
                Logger.Error(e, LogType.Runtime);
            }
        })
        {
        }
    }
}
