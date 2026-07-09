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
        ///     Refreshes the selected map's online ranked status from /v2/map/:id.
        /// </summary>
        /// <param name="map"></param>
        public static void UpdateRankedStatus(Map map)
        {
            if (map == null || map.MapId <= 0 || OnlineManager.Client == null)
                return;

            var response = OnlineManager.Client.RetrieveMapInfo(map.MapId);

            if (response?.Map == null)
                return;

            var rankedStatus = response.Map.RankedStatus;

            if (map.RankedStatus == rankedStatus)
                return;

            map.RankedStatus = rankedStatus;
            MapDatabaseCache.UpdateMap(map);
            Logger.Debug($"Updated online ranked status for map {map.MapId} to {rankedStatus}", LogType.Runtime);
            RankedStatusUpdated?.Invoke(null, EventArgs.Empty);
        }
    }
}
