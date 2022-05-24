using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Client.Events.Scores;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
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

                var mods = ModHelper.GetModsFromRate(ModHelper.GetRateFromMods(ModManager.Mods));

                if (mods == ModIdentifier.None)
                    mods = 0;

                mods = ModManager.Mods - (long) mods;

                var onlineScores = OnlineManager.Client?.RetrieveScoreboard(map.MapId, map.Md5Checksum, OnlineScoreboard.Mods, mods);
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