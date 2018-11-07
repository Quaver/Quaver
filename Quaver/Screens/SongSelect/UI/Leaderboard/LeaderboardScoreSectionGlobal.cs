using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Online;
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
        public override List<LocalScore> FetchScores()
        {
            if (!OnlineManager.Connected)
                return new List<LocalScore>();

            var map = MapManager.Selected.Value;

            // Get previously cached scores.
            if (ScoreCache.ContainsKey(map))
            {
                Logger.Debug($"Already have previous global scores. Fetching from cache.", LogType.Runtime, false);
                return ScoreCache[map];
            }

            var onlineScores = OnlineManager.Client?.RetrieveOnlineScores(map.MapId, map.Md5Checksum);

            var scores = new List<LocalScore>();

            if (onlineScores?.Scores == null)
            {
                ScoreCache[map] = scores;
                return scores;
            }

            foreach (var score in onlineScores.Scores)
                scores.Add(LocalScore.FromOnlineScoreboardScore(score));

            ScoreCache[map] = scores;
            return scores;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public override string GetNoScoresAvailableString(Map map)
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            return map.RankedStatus == RankedStatus.Ranked
                ? "No scores submitted. Be the first!"
                : "No scores avaialable. This map isn't ranked!";
        }
    }
}