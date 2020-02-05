using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.Shared.Database.Scores;

namespace Quaver.Shared.Database.Profiles
{
    public class UserProfileStats
    {
        /// <summary>
        /// </summary>
        public UserProfile Profile { get; }

        /// <summary>
        /// </summary>
        public GameMode Mode { get; }

        /// <summary>
        /// </summary>
        public double OverallRating { get; set; }

        /// <summary>
        /// </summary>
        public double OverallAccuracy { get; set; }

        /// <summary>
        /// </summary>
        public long TotalScore { get; set; }

        /// <summary>
        /// </summary>
        public int MaxCombo { get; set; }

        /// <summary>
        /// </summary>
        public int PlayCount { get; set; }

        /// <summary>
        /// </summary>
        public List<Score> Scores { get; set; }

        /// <summary>
        /// </summary>
        public Dictionary<Judgement, int> JudgementCounts { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="mode"></param>
        public UserProfileStats(UserProfile profile, GameMode mode)
        {
            Profile = profile;
            Mode = mode;

            FetchStats();
        }

        /// <summary>
        ///     Handles populating the stats and scores for these stats
        /// </summary>
        public void FetchStats()
        {
            if (Profile.IsOnline)
                PopulateOnlineStats();
            else
                PopulateLocalStats();
        }

        /// <summary>
        ///     Fetches statistics from online
        /// </summary>
        private void PopulateOnlineStats()
        {
        }

        /// <summary>
        ///     Fetches statistics and scores from the local database
        /// </summary>
        private void PopulateLocalStats()
        {
            Scores = DatabaseManager.Connection?.Table<Score>()
                .ToList()
                .FindAll(x => x.UserProfileId == Profile.Id && x.Mode == Mode)
                .ToList();

            if (Scores == null)
                Scores = new List<Score>();

            Scores = Scores.OrderByDescending(x => x.PerformanceRating).ToList();

            PlayCount = Scores.Count;

            CalculateOverallRating();
            CalculateOverallAccuracy();
            CalculateJudgementsMaxComboAndTotalScore();
        }

        /// <summary>
        ///     Calculates overall rating from the list of scores
        /// </summary>
        private void CalculateOverallRating() => OverallRating = Scores.Select((t, i) => t.PerformanceRating * Math.Pow(0.95, i)).Sum();

        /// <summary>
        ///     Calculates overall accuracy from the list of scores
        /// </summary>
        private void CalculateOverallAccuracy()
        {
            var total = 0d;
            var divideTotal = 0d;

            for (var i = 0; i < Scores.Count; i++)
            {
                if (Scores[i].Grade == Grade.F)
                    continue;

                var add = Math.Pow(0.95f, i) * 100;
                total += Scores[i].Accuracy * add;
                divideTotal += add;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (divideTotal == 0)
                return;

            OverallAccuracy = total / divideTotal;
        }

        /// <summary>
        /// </summary>
        private void CalculateJudgementsMaxComboAndTotalScore()
        {
            JudgementCounts = new Dictionary<Judgement, int>();

            foreach (Judgement j in Enum.GetValues(typeof(Judgement)))
            {
                if (j == Judgement.Ghost)
                    continue;

                JudgementCounts.Add(j, 0);
            }

            foreach (var score in Scores)
            {
                MaxCombo = Math.Max(MaxCombo, score.MaxCombo);
                TotalScore += score.TotalScore;

                JudgementCounts[Judgement.Marv] += score.CountMarv;
                JudgementCounts[Judgement.Perf] += score.CountPerf;
                JudgementCounts[Judgement.Great] += score.CountGreat;
                JudgementCounts[Judgement.Good] += score.CountGood;
                JudgementCounts[Judgement.Okay] += score.CountOkay;
                JudgementCounts[Judgement.Miss] += score.CountMiss;
            }
        }
    }
}