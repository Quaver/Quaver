/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;

namespace Quaver.Shared.Screens.Select.UI.Leaderboard
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
        public override FetchedScoreStore FetchScores()
        {
            var map = MapManager.Selected.Value;

            var scores = ScoreDatabaseCache.FetchMapScores(map.Md5Checksum);
            return new FetchedScoreStore(scores);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public override string GetNoScoresAvailableString(Map map) => "No local scores available. Be the first!";
    }
}
