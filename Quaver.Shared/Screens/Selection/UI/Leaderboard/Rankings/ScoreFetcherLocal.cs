using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.API.Maps.Processors.Rating;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
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

                if (map.DifficultyProcessorVersion == DifficultyProcessorKeys.Version)
                {
                    foreach (var score in scores)
                    {
                        if (score.DifficultyProcessorVersion == DifficultyProcessorKeys.Version)
                            continue;

                        score.DifficultyProcessorVersion = DifficultyProcessorKeys.Version;
                        score.RatingProcessorVersion = RatingProcessorKeys.Version;
                        var oldRating = score.PerformanceRating;

                        var diff = map.DifficultyFromMods((ModIdentifier) score.Mods);
                        var rating = new RatingProcessorKeys(diff).CalculateRating(score.Accuracy, score.Grade == Grade.F);

                        score.PerformanceRating = rating;
                        ScoreDatabaseCache.UpdateScore(score);

                        Logger.Important($"Rating of score: {score.Id} recalculated to {score.PerformanceRating} from {oldRating}",
                            LogType.Runtime, false);
                    }
                }

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