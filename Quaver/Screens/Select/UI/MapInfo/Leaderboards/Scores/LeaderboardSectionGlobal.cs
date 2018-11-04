using System.Collections.Generic;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Online;

namespace Quaver.Screens.Select.UI.MapInfo.Leaderboards.Scores
{
    public class LeaderboardSectionGlobal : LeaderboardSectionScores
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="leaderboard"></param>
        public LeaderboardSectionGlobal(Leaderboard leaderboard) : base(LeaderboardSectionType.Global, leaderboard, "Global")
        {
            ScrollContainer.Alpha = 0;

            FetchAndUpdateLeaderboards(null);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected sealed override List<LocalScore> FetchScores()
        {
            if (!OnlineManager.Connected)
                return new List<LocalScore>();

            var scores = OnlineManager.Client.RetrieveOnlineScores(MapManager.Selected.Value.MapId, MapManager.Selected.Value.Md5Checksum);

            var localScores = new List<LocalScore>();
            scores?.Scores?.ForEach(x => localScores.Add(LocalScore.FromOnlineScoreboardScore(x)));

            return localScores;
        }
    }
}