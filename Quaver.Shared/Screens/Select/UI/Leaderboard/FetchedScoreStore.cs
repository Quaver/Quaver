using System.Collections.Generic;
using Quaver.Shared.Database.Scores;

namespace Quaver.Shared.Screens.Select.UI.Leaderboard
{
    public struct FetchedScoreStore
    {
        public List<Score> Scores { get; }
        public Score PersonalBest { get; }

        public FetchedScoreStore(List<Score> scores, Score personalBest = null)
        {
            Scores = scores;
            PersonalBest = personalBest;
        }
    }
}