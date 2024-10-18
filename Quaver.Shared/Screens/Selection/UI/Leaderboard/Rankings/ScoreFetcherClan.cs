using Quaver.Server.Client.Events.Scores;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Rankings
{
    using System;
    using System.Collections.Generic;
    using Wobble.Logging;

    namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Rankings
    {
        /// <summary>
        ///     Fetches scores for <see cref="LeaderboardType.All"/>
        /// </summary>
        public class ScoreFetcherClan : IScoreFetcher
        {
            public FetchedScoreStore Fetch(Map map)
            {
                try
                {
                    if (!OnlineManager.Connected)
                        return new FetchedScoreStore(new List<Score>());

                    var onlineScores = OnlineManager.Client?.GetClanScoreboard(map.Md5Checksum);
                    
                    var scores = new List<Score>();

                    if (onlineScores?.Scores == null)
                        return new FetchedScoreStore(new List<Score>());

                    foreach (var score in onlineScores.Scores)
                        scores.Add(Score.FromOnlineScoreboardScore(score));
                    
                    return new FetchedScoreStore(scores, null);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    return new FetchedScoreStore(new List<Score>());
                }
            }
        }
    }
}