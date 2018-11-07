using System.Collections.Generic;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Wobble.Logging;

namespace Quaver.Screens.SongSelect.UI.Leaderboard
{
    public class LeaderboardScoreSectionLocal : LeaderboardScoreSection
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override LeaderboardType Type { get; } = LeaderboardType.Local;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="leaderboard"></param>
        public LeaderboardScoreSectionLocal(LeaderboardContainer leaderboard) : base(leaderboard)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override List<LocalScore> FetchScores()
        {
            if (ScoreCache.ContainsKey(MapManager.Selected.Value))
            {
                Logger.Debug($"Already have previous local scores. Fetching from cache.", LogType.Runtime, false);
                return ScoreCache[MapManager.Selected.Value];
            }

            var scores = LocalScoreCache.FetchMapScores(MapManager.Selected.Value.Md5Checksum);
            ScoreCache[MapManager.Selected.Value] = scores;

            return scores;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public override string GetNoScoresAvailableString(Map map) => "No local scores available. Be the first!";
    }
}