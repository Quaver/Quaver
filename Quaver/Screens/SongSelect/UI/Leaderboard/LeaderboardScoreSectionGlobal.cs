using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Online;
using Quaver.Server.Client.Events.Scores;
using Wobble.Logging;

namespace Quaver.Screens.SongSelect.UI.Leaderboard
{
    public class LeaderboardScoreSectionGlobal : LeaderboardScoreSection
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override LeaderboardType Type { get; } = LeaderboardType.Global;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="leaderboard"></param>
        public LeaderboardScoreSectionGlobal(LeaderboardContainer leaderboard) : base(leaderboard)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override FetchedScoreStore FetchScores()
        {
            if (!OnlineManager.Connected)
                return new FetchedScoreStore(new List<LocalScore>());

            var map = MapManager.Selected.Value;

            // Get previously cached scores.
            if (ScoreCache.ContainsKey(map))
                return ScoreCache[map];

            var onlineScores = OnlineManager.Client?.RetrieveOnlineScores(map.MapId, map.Md5Checksum);
            map.NeedsOnlineUpdate = onlineScores?.Code == OnlineScoresResponseCode.NeedsUpdate;

            var scores = new List<LocalScore>();

            if (onlineScores?.Scores == null)
            {
                ScoreCache[map] = new FetchedScoreStore(new List<LocalScore>());
                return ScoreCache[map];
            }

            foreach (var score in onlineScores.Scores)
                scores.Add(LocalScore.FromOnlineScoreboardScore(score));

            var pb = onlineScores.PersonalBest != null ? LocalScore.FromOnlineScoreboardScore(onlineScores.PersonalBest) : null;

            ScoreCache[map] = new FetchedScoreStore(scores, pb);
            return ScoreCache[map];
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public override string GetNoScoresAvailableString(Map map)
        {
            if (!OnlineManager.Connected)
                return "You must be logged in to view online scores!";

            if (map.NeedsOnlineUpdate)
                return "Your map is outdated. Please update it!";

            switch (map.RankedStatus)
            {
                case RankedStatus.NotSubmitted:
                    return "This map is not submitted online!";
                case RankedStatus.Unranked:
                    return "This map is not ranked!";
                case RankedStatus.Ranked:
                    return "No scores available. Be the first!";
                case RankedStatus.DanCourse:
                    return "Unavailable!";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}