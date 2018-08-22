using System;
using System.Collections.Generic;
using System.Threading;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Scheduling;

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
        // ReSharper disable once ArrangeConstructorOrDestructorBody
        {
            // ReSharper disable once ArrangeConstructorOrDestructorBody
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
        private void CreateLeaderboardScores(IReadOnlyList<LocalScore> scores)
        {
            for (var i = 0; i < scores.Count; i++)
            {
                var score = new LeaderboardScore(this, scores[i], i + 1);
                score.Y = i * score.Height + i * 5;

                LeaderboardScores.Add(score);
            }
        }

        /// <summary>
        ///     Clears the leaderboard.
        /// </summary>
        private void ClearLeaderboard()
        {
            LeaderboardScores.ForEach(x => x.Destroy());
            LeaderboardScores.Clear();
        }

        /// <summary>
        ///     Fetches the scores and updates the leaderboards.
        /// </summary>
        public void FetchAndUpdateLeaderboards()
        {
            ClearLeaderboard();

            // ReSharper disable once ArrangeMethodOrOperatorBody
            Scheduler.RunThread(() =>
            {
                // If we already have scores cached, then use them.
                if (MapManager.Selected.Value.Scores.Value != null && MapManager.Selected.Value.Scores?.Value?.Count != 0)
                {
                    CreateLeaderboardScores(MapManager.Selected.Value.Scores.Value);
                    return;
                }

                // Grab the map before fetching the scores, so we can know if to update it or not.
                var mapBeforeFetching = MapManager.Selected.Value;
                mapBeforeFetching.Scores.Value = FetchScores();

                // If the map is still the same after fetching, we'll want to create leaderboard scores.
                if (MapManager.Selected.Value == mapBeforeFetching)
                    CreateLeaderboardScores(mapBeforeFetching.Scores.Value);
            });
        }
    }
}