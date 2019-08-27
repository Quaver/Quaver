using System;
using System.Collections.Generic;
using Quaver.Server.Client.Events.Scores;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Select.UI.Leaderboard;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Rankings
{
    public class ScoreFetcherMods : IScoreFetcher
    {
        public FetchedScoreStore Fetch(Map map)
        {
            try
            {
                if (!OnlineManager.Connected)
                    return new FetchedScoreStore(new List<Score>());

                var onlineScores = OnlineManager.Client?.RetrieveOnlineScores(map.MapId, map.Md5Checksum, ModManager.Mods);
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