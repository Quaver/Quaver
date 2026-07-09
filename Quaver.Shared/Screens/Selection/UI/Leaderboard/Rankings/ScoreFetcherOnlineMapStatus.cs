using System;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Online;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Rankings
{
    internal static class ScoreFetcherOnlineMapStatus
    {
        public static event EventHandler RankedStatusUpdated;

        /// <summary>
        ///     Refreshes the selected map's online status from /v2/map/:id.
        /// </summary>
        /// <param name="map"></param>
        public static void UpdateMapStatus(Map map)
        {
            if (map == null || map.MapId <= 0 || OnlineManager.Client == null)
                return;

            var response = OnlineManager.Client.RetrieveMapInfo(map.MapId);

            if (response?.Map == null)
                return;

            var rankedStatus = response.Map.RankedStatus;
            var needsOnlineUpdate = NeedsOnlineUpdate(map, response.Map.Md5);

            var rankedStatusChanged = map.RankedStatus != rankedStatus;
            var needsOnlineUpdateChanged = map.NeedsOnlineUpdate != needsOnlineUpdate;

            if (!rankedStatusChanged && !needsOnlineUpdateChanged)
                return;

            map.RankedStatus = rankedStatus;
            map.NeedsOnlineUpdate = needsOnlineUpdate;

            if (rankedStatusChanged)
                MapDatabaseCache.UpdateMap(map);

            Logger.Debug($"Updated online map status for map {map.MapId}: ranked={rankedStatus}, needsUpdate={needsOnlineUpdate}", LogType.Runtime);
            RankedStatusUpdated?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="onlineMd5"></param>
        /// <returns></returns>
        private static bool NeedsOnlineUpdate(Map map, string onlineMd5)
        {
            if (string.IsNullOrEmpty(onlineMd5))
                return false;

            if (string.Equals(map.Md5Checksum, onlineMd5, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }
    }
}
