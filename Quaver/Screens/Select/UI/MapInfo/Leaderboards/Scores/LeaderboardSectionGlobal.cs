using System.Collections.Generic;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;

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

            // TOOD: REPLACE WITH ONLINE SCORES
            FetchAndUpdateLeaderboards(null);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected sealed override List<LocalScore> FetchScores() => new List<LocalScore>();
    }
}