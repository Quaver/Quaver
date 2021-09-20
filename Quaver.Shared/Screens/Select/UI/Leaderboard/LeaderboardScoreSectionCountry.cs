using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Server.Client.Events.Scores;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Select.UI.Leaderboard
{
    public class LeaderboardScoreSectionCountry : LeaderboardScoreSection
    {
        public override LeaderboardType Type { get; } = LeaderboardType.Country;

        public LeaderboardScoreSectionCountry(LeaderboardContainer leaderboard) : base(leaderboard)
        {
        }

        public override FetchedScoreStore FetchScores()
        {
            /*if (!OnlineManager.Connected || !OnlineManager.IsDonator)
                return new FetchedScoreStore(new List<Score>());

            var map = MapManager.Selected.Value;

            var onlineScores = OnlineManager.Client?.RetrieveOnlineScores(map.MapId, map.Md5Checksum, ModIdentifier.None, true);
            map.NeedsOnlineUpdate = onlineScores?.Code == OnlineScoresResponseCode.NeedsUpdate;

            var scores = new List<Score>();

            if (onlineScores?.Scores == null)
                return new FetchedScoreStore(new List<Score>());

            foreach (var score in onlineScores.Scores)
                scores.Add(Score.FromOnlineScoreboardScore(score));

            var pb = onlineScores.PersonalBest != null ? Score.FromOnlineScoreboardScore(onlineScores.PersonalBest) : null;*/
            return new FetchedScoreStore(null, null);
        }

        public override string GetNoScoresAvailableString(Map map) => LeaderboardScoreSectionGlobal.GetNoScoresAvailable(map);
    }
}