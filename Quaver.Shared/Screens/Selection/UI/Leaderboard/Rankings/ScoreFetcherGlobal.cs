using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Server.Client.Events.Scores;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Online;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Rankings
{
    /// <summary>
    ///     Fetches scores for <see cref="LeaderboardType.Global"/>
    /// </summary>
    public class ScoreFetcherGlobal : IScoreFetcher
    {
        public FetchedScoreStore Fetch(Map map)
        {
            try
            {
                if (!OnlineManager.Connected)
                    return new FetchedScoreStore(new List<Score>());

                var onlineScores = OnlineManager.Client?.RetrieveScoreboard(map.MapId, map.Md5Checksum, OnlineScoreboard.Global);

                map.NeedsOnlineUpdate = onlineScores?.Code == OnlineScoresResponseCode.NeedsUpdate;

                var scores = new List<Score>();

                if (onlineScores?.Scores == null)
                    return new FetchedScoreStore(new List<Score>());

                foreach (var score in onlineScores.Scores)
                    scores.Add(Score.FromOnlineScoreboardScore(score));

                var pb = onlineScores.PersonalBest != null ? Score.FromOnlineScoreboardScore(onlineScores.PersonalBest) : null;
                return new FetchedScoreStore(scores, pb);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                return new FetchedScoreStore(new List<Score>());
            }
        }
    }
}