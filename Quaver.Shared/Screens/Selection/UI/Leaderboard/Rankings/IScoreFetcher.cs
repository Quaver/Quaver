using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.Select.UI.Leaderboard;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Rankings
{
    public interface IScoreFetcher
    {
        /// <summary>
        ///     Fetches scores from an individual map
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        FetchedScoreStore Fetch(Map map);
    }
}