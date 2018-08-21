using System;
using System.Collections.Generic;
using Quaver.Database.Scores;

namespace Quaver.Screens.Select.UI.MapInfo.Leaderboards.Scores
{
    public abstract class LeaderboardSectionScores : LeaderboardSection
    {
        /// <summary>
        ///     The current leaderboard scores displayed.
        /// </summary>
        public List<LeaderboardScore> LeaderboardScores { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="rankingSection"></param>
        /// <param name="leaderboard"></param>
        /// <param name="name"></param>
        protected LeaderboardSectionScores(LeaderboardRankingSection rankingSection, Leaderboard leaderboard,
            string name) : base(rankingSection, leaderboard, name)
        {
            LeaderboardScores = new List<LeaderboardScore>();
        }

        /// <summary>
        ///     Fetches scores to use for the leaderboard.
        /// </summary>
        /// <returns></returns>
        protected abstract List<LocalScore> FetchScores();

        /// <summary>
        ///     Creates the drawables for leaderboard scores.
        /// </summary>
        /// <returns></returns>
        protected void CreateLeaderboardScores(List<LocalScore> scores)
        {
            LeaderboardScores.ForEach(x => x.Destroy());
            LeaderboardScores = new List<LeaderboardScore>();

            for (var i = 0; i < scores.Count; i++)
            {
                var score = new LeaderboardScore(this, scores[i], i + 1);
                score.Y = i * score.Height + i * 5;
            }
        }
    }
}