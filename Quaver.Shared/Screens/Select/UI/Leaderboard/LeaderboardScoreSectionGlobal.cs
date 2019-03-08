/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Server.Client.Events.Scores;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Select.UI.Leaderboard
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
                return new FetchedScoreStore(new List<Score>());

            var map = MapManager.Selected.Value;

            var onlineScores = OnlineManager.Client?.RetrieveOnlineScores(map.MapId, map.Md5Checksum);
            map.NeedsOnlineUpdate = onlineScores?.Code == OnlineScoresResponseCode.NeedsUpdate;

            var scores = new List<Score>();

            if (onlineScores?.Scores == null)
                return new FetchedScoreStore(new List<Score>());

            foreach (var score in onlineScores.Scores)
                scores.Add(Score.FromOnlineScoreboardScore(score));

            var pb = onlineScores.PersonalBest != null ? Score.FromOnlineScoreboardScore(onlineScores.PersonalBest) : null;
            return new FetchedScoreStore(scores, pb);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public override string GetNoScoresAvailableString(Map map) => GetNoScoresAvailable(map);

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string GetNoScoresAvailable(Map map)
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
