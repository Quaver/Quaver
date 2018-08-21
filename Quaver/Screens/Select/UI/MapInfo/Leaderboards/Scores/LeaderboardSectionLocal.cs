using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.Database.Maps;
using Quaver.Database.Scores;

namespace Quaver.Screens.Select.UI.MapInfo.Leaderboards.Scores
{
    public class LeaderboardSectionLocal : LeaderboardSectionScores
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="leaderboard"></param>
        public LeaderboardSectionLocal(Leaderboard leaderboard) : base(LeaderboardRankingSection.Local, leaderboard, "Local")
        {
            ContentContainer.Alpha = 0;
            var scores = FetchScores();

            for (var i = 0; i < scores.Count; i++)
            {
                Console.WriteLine($"[{i}] - {scores[i].Name} - {scores[i].Grade} - {scores[i].Score} - {scores[i].Accuracy} - {scores[i].Mods}");
            }

            CreateLeaderboardScores(scores);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected sealed override List<LocalScore> FetchScores() => LocalScoreCache.FetchMapScores(MapManager.Selected.Value.Md5Checksum);
    }
}