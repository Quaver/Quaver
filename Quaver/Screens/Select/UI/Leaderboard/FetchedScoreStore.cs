using System.Collections.Generic;
using Quaver.Database.Scores;

namespace Quaver.Screens.Select.UI.Leaderboard
{
    public struct FetchedScoreStore
    {
        public List<LocalScore> Scores { get; }
        public LocalScore PersonalBest { get; }

        public FetchedScoreStore(List<LocalScore> scores, LocalScore personalBest = null)
        {
            Scores = scores;
            PersonalBest = personalBest;
        }
    }
}