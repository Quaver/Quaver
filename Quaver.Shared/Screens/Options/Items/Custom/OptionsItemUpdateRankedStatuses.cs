using System;
using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.Ranked;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemUpdateRankedStatuses : OptionsItem
    {
        /// <summary>
        ///     Invoked when map ranked statuses are being checked.
        /// </summary>
        public static event EventHandler<MapDatabaseCacheProgressEventArgs> RankedStatusUpdateProgress;

        /// <summary>
        /// </summary>
        private IconButton Button { get; }

        /// <summary>
        ///     If the task is currently running
        /// </summary>
        private static bool IsRunning { get; set; }

        public OptionsItemUpdateRankedStatuses(RectangleF containerRect, string name) : base(containerRect, name)
        {
            const float scale = 0.85f;

            Button = new IconButton(UserInterface.OptionsUpdateButton)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                UsePreviousSpriteBatchOptions = true
            };

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

            ThreadScheduler.Run(() =>
            {
                try
                {
                    var count = UpdateRankedStatuses();

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

        /// <summary>
        ///     Updates map ranked statuses immediately on the current thread.
        /// </summary>
        /// <returns></returns>
        public static int UpdateRankedStatuses()
        {
            var count = 0;
            RankedStatusUpdateProgress?.Invoke(typeof(OptionsItemUpdateRankedStatuses),
                new MapDatabaseCacheProgressEventArgs("Fetching ranked statuses", "Online ranked mapsets", 0, 1));

            var response = new APIRequestRankedMapsets().ExecuteRequest();
            var hashSet = response.Mapsets.ToHashSet();
            var maps = new List<Map>();

            foreach (var mapset in MapManager.Mapsets)
            {
                if (mapset.Maps.Count == 0)
                    continue;

                if (mapset.Maps.First().Game != MapGame.Quaver)
                    continue;

                maps.AddRange(mapset.Maps);
            }

            Logger.Important($"There are currently {response.Mapsets.Count} ranked mapsets to check.", LogType.Runtime);

            for (var i = 0; i < maps.Count; i++)
            {
                var map = maps[i];
                RankedStatusUpdateProgress?.Invoke(typeof(OptionsItemUpdateRankedStatuses),
                    new MapDatabaseCacheProgressEventArgs("Updating ranked status", map.Path, i, maps.Count));

                if (map.MapId == -1 || !hashSet.Contains(map.MapSetId) || map.RankedStatus == RankedStatus.Ranked)
                    continue;

                map.RankedStatus = RankedStatus.Ranked;
                MapDatabaseCache.UpdateMap(map);
                count++;
            }

            return count;
        }
    }
}
