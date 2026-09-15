using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.Offsets;
using Quaver.Shared.Online.API.Ranked;
using Quaver.Shared.Scheduling;
using Wobble.Logging;

namespace Quaver.Shared.Database.Maps
{
    /// <summary>
    ///     Updates the online offsets and ranked statuses of the installed maps from the server, in the
    ///     background. Used by the buttons in both options menus, and by the importer after an import.
    /// </summary>
    public static class MapMetadataUpdater
    {
        private static bool IsUpdatingOnlineOffsets { get; set; }

        /// <summary>
        ///     Only set by updates the user started, so an import never blocks the options button.
        /// </summary>
        private static bool IsUpdatingRankedStatuses { get; set; }

        /// <summary>
        ///     Updates online offsets for the user. Shows a warning instead when an update is already
        ///     running or no maps are loaded.
        /// </summary>
        public static void UpdateOnlineOffsets()
        {
            if (IsUpdatingOnlineOffsets)
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

            IsUpdatingOnlineOffsets = true;

            NotificationManager.Show(NotificationLevel.Info,
                "Your maps' online offsets are now being updated in the background...");

            var mapsets = new List<Mapset>(MapManager.Mapsets);

            ThreadScheduler.Run(() =>
            {
                try
                {
                    var count = 0;

                    var response = new APIRequestOnlineOffsets().ExecuteRequest();
                    var mapDict = response.Maps.ToDictionary(x => x.Id, y => y.Offset);

                    Logger.Important($"There are currently {mapDict.Count} maps to check.", LogType.Runtime);

                    foreach (var mapset in mapsets)
                    {
                        if (mapset.Maps.Count == 0)
                            continue;

                        if (mapset.Maps.First().Game != MapGame.Quaver)
                            continue;

                        foreach (var map in mapset.Maps)
                        {
                            if (map.MapId == -1 || !mapDict.ContainsKey(map.MapId))
                                continue;

                            map.OnlineOffset = mapDict[map.MapId];
                            MapDatabaseCache.UpdateMap(map);
                            count++;
                        }
                    }

                    NotificationManager.Show(NotificationLevel.Success,
                        $"Successfully updated the online offsets of {count:n0} maps!");
                    Logger.Important($"Finished updating offsets of: {count} maps", LogType.Runtime);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error,
                        "There was an issue while updating your maps' offsets.");
                }
                finally
                {
                    IsUpdatingOnlineOffsets = false;
                }
            });
        }

        /// <summary>
        ///     Updates ranked statuses for the user. Shows a warning instead when an update is already
        ///     running or no maps are loaded.
        /// </summary>
        public static void UpdateRankedStatuses()
        {
            if (IsUpdatingRankedStatuses)
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

            RunRankedStatusUpdate(true);
        }

        /// <summary>
        ///     Updates ranked statuses after an import, without the checks above.
        /// </summary>
        public static void UpdateRankedStatusesInBackground() => RunRankedStatusUpdate(false);

        /// <param name="userInitiated">Whether the user started this update from a button.</param>
        private static void RunRankedStatusUpdate(bool userInitiated)
        {
            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Warning, "Cannot update ranked statuses when not online.");
                return;
            }

            if (userInitiated)
                IsUpdatingRankedStatuses = true;

            NotificationManager.Show(NotificationLevel.Info,
                "Your maps' ranked statuses are now being updated in the background...");

            var mapsets = new List<Mapset>(MapManager.Mapsets);

            ThreadScheduler.Run(() =>
            {
                try
                {
                    var count = 0;
                    var response = new APIRequestRankedMapsets().ExecuteRequest();
                    var hashSet = response.Mapsets.ToHashSet();

                    Logger.Important($"There are currently {response.Mapsets.Count} ranked mapsets to check.",
                        LogType.Runtime);

                    foreach (var mapset in mapsets)
                    {
                        if (mapset.Maps.Count == 0)
                            continue;

                        if (mapset.Maps.First().Game != MapGame.Quaver)
                            continue;

                        foreach (var map in mapset.Maps)
                        {
                            if (map.MapId == -1 || !hashSet.Contains(map.MapSetId) ||
                                map.RankedStatus == RankedStatus.Ranked)
                                continue;

                            map.RankedStatus = RankedStatus.Ranked;
                            MapDatabaseCache.UpdateMap(map);
                            count++;
                        }
                    }

                    NotificationManager.Show(NotificationLevel.Success,
                        $"Successfully updated the ranked statuses of {count:n0} maps!");
                    Logger.Important($"Finished updating statuses of: {count} maps", LogType.Runtime);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error,
                        "There was an issue while updating your maps' ranked statuses");
                }
                finally
                {
                    if (userInitiated)
                        IsUpdatingRankedStatuses = false;
                }
            });
        }
    }
}
