using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Wobble.Graphics.Buttons;
using Quaver.Shared.Graphics.Notifications;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.Ranked;
using Quaver.Shared.Scheduling;
using Wobble.Graphics;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemUpdateRankedStatuses : OptionsItem
    {
        /// <summary>
        /// </summary>
        private RoundedButton Button { get; }

        /// <summary>
        ///     If the task is currently running
        /// </summary>
        private static bool IsRunning { get; set; }

        public OptionsItemUpdateRankedStatuses(RectangleF containerRect, string name) : base(containerRect, name)
        {
            const float scale = 0.85f;

            Button = new RoundedButton
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                Tint = ColorHelper.HexToColor("#F2994A")
            };

            Button.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "UPDATE", 18, Color.White);

            Button.Clicked += (sender, args) =>
            {
                if (IsRunning)
                {
                    NotificationManager.Show(NotificationLevel.Warning, "Your maps are already being updated! " +
                                                                        "Please wait until it has completed!");
                    return;
                }

                if (MapManager.Mapsets.Count == 0)
                {
                    NotificationManager.Show(NotificationLevel.Warning, "You do not have any maps loaded!");
                    return;
                }

                Run();
            };
        }

        public static void Run(bool fromOptions = true)
        {
            // Don't run if client is not connected
            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Warning, "Cannot update ranked statuses when not online.");
                return;
            }

            if (fromOptions)
                IsRunning = true;

            NotificationManager.Show(NotificationLevel.Info, "Your maps' ranked statuses are now being updated in the background...");

            var mapsets = new List<Mapset>(MapManager.Mapsets);

            ThreadScheduler.Run(() =>
            {
                try
                {
                    var count = 0;
                    var response = new APIRequestRankedMapsets().ExecuteRequest();
                    var hashSet = response.Mapsets.ToHashSet();

                    Logger.Important($"There are currently {response.Mapsets.Count} ranked mapsets to check.", LogType.Runtime);

                    foreach (var mapset in mapsets)
                    {
                        if (mapset.Maps.Count == 0)
                            continue;

                        if (mapset.Maps.First().Game != MapGame.Quaver)
                            continue;

                        foreach (var map in mapset.Maps)
                        {
                            if (map.MapId == -1 || !hashSet.Contains(map.MapSetId) || map.RankedStatus == RankedStatus.Ranked)
                                continue;

                            map.RankedStatus = RankedStatus.Ranked;
                            MapDatabaseCache.UpdateMap(map);
                            count++;
                        }
                    }

                    NotificationManager.Show(NotificationLevel.Success, $"Successfully updated the ranked statuses of {count:n0} maps!");
                    Logger.Important($"Finished updating statuses of: {count} maps", LogType.Runtime);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "There was an issue while updating your maps' ranked statuses");
                }
                finally
                {
                    if (fromOptions)
                        IsRunning = false;
                }
            });
        }
    }
}
