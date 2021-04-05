using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq.Extensions;
using Quaver.API.Maps;
using Quaver.API.Maps.AutoMod;
using Quaver.API.Maps.AutoMod.Issues;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Edit.UI.AutoMods;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorSubmitForRankDialog : LoadingDialog
    {
        public EditorSubmitForRankDialog(EditScreen screen) : base("SUBMIT FOR RANK", $"Please wait while your map is being submitted for rank...", () =>
        {
            // Run AutoMod and search for any issues.
            var autoMod = new AutoModMapset(screen.Map.Mapset.Maps.Select(x => x.LoadQua()).ToList());
            autoMod.Run();

            var mapsetIssues = autoMod.Issues.Any(x => x.Level > AutoModIssueLevel.Warning);
            var mapIssues = autoMod.Mods.Any(x => x.Value.Issues.Any(y => y.Level > AutoModIssueLevel.Warning));

            if (mapsetIssues || mapIssues)
            {
                NotificationManager.Show(NotificationLevel.Error, $"Your mapset has issues that prevent it from being ranked. Please " +
                                                                  $"review the AutoMod for each difficulty, and then try again.");

                var view = screen.View as EditScreenView;

                if (view != null)
                    view.AutoMod.IsActive.Value = true;

                return;
            }

            // Lastly, submit for rank.
        })
        {
        }
    }
}