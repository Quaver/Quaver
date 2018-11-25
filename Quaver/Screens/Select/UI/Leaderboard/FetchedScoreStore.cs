using System.Collections.Generic;
using Quaver.Database.Scores;

namespace Quaver.Screens.Select.UI.Leaderboard
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