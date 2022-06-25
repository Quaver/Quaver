using Quaver.Shared.Database.Maps;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Components
{
    public interface IFetchedScoreHandler
    {
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="store"></param>
        void HandleFetchedScores(Map map, FetchedScoreStore store);
    }
}