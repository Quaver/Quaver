using Quaver.API.Maps.Processors.Difficulty;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Server.Client.Structures;

namespace Quaver.Shared.Screens.Tournament.Overlay
{
    public class TournamentPlayer
    {
        /// <summary>
        /// </summary>
        public User User { get; }

        /// <summary>
        /// </summary>
        public ScoreProcessor Scoring { get; }

        /// <summary>
        /// </summary>
        public RatingProcessorKeys Rating { get; }

        /// <summary>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="scoring"></param>
        /// <param name="difficultyRating"></param>
        public TournamentPlayer(User user, ScoreProcessor scoring, float difficultyRating)
        {
            User = user;
            Scoring = scoring;
            Rating = new RatingProcessorKeys(difficultyRating);
        }
    }
}