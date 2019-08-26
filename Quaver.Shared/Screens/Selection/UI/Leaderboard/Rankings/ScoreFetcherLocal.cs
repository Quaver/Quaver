using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Screens.Select.UI.Leaderboard;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Rankings
{
    /// <summary>
    ///     Fetches scores for <see cref="LeaderboardType.Local"/>
    /// </summary>
    public class ScoreFetcherLocal : IScoreFetcher
    {
        public FetchedScoreStore Fetch(Map map)
        {
            try
            {
                var scores = ScoreDatabaseCache.FetchMapScores(map.Md5Checksum);
                return new FetchedScoreStore(scores, scores?.Count != 0 ? scores?.First() : null);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                return new FetchedScoreStore(new List<Score>());
            }
        }
    }
}