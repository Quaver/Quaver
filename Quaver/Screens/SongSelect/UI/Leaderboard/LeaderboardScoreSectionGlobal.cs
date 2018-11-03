using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Online;

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

            var onlineScores = OnlineManager.Client?.RetrieveOnlineScores(MapManager.Selected.Value.MapId, MapManager.Selected.Value.Md5Checksum);

            var scores = new List<LocalScore>();

            if (onlineScores?.Scores == null)
                return scores;

            foreach (var score in onlineScores.Scores)
                scores.Add(LocalScore.FromOnlineScoreboardScore(score));

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