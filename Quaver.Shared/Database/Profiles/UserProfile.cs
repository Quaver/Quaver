using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.User;
using SQLite;

namespace Quaver.Shared.Database.Profiles
{
    public class UserProfile
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// </summary>
        [Ignore]
        public Dictionary<GameMode, UserProfileStats> Stats { get; } = new Dictionary<GameMode, UserProfileStats>();

        /// <summary>
        /// </summary>
        [Ignore]
        public bool IsOnline { get; set; }

        /// <summary>
        /// </summary>
        public UserProfile PopulateStats()
        {
            foreach (GameMode mode in ModeHelper.AllModes)
            {
                if (!IsOnline && Stats.ContainsKey(mode))
                {
                    Stats[mode].FetchStats();
                    continue;
                }

                if (!Stats.ContainsKey(mode))
                    Stats.Add(mode, new UserProfileStats(this, mode));
            }

            // Fetch stats for the online profile
            if (IsOnline && OnlineManager.Connected)
            {
                var stats = new APIRequestUsersFull(OnlineManager.Self.OnlineUser.Id).ExecuteRequest();

                foreach (GameMode mode in ModeHelper.AllModes)
                {
                    APIResponseUsersFullMode modeStats = null;

                    switch (mode)
                    {
                        case GameMode.Keys4:
                            modeStats = stats.User.StatsKeys4;
                            break;
                        case GameMode.Keys7:
                            modeStats = stats.User.StatsKeys7;
                            break;
                        default:
                            continue;
                    }

                    var currentMode = Stats[mode];

                    currentMode.GlobalRank = modeStats.Ranks.Global;
                    currentMode.CountryRank = modeStats.Ranks.Country;
                    currentMode.OverallRating = modeStats.OverallPerformanceRating;
                    currentMode.OverallAccuracy = modeStats.OverallAccuracy;
                    currentMode.TotalScore = modeStats.RankedScore;
                    currentMode.MaxCombo = modeStats.MaxCombo;
                    currentMode.PlayCount = modeStats.PlayCount;
                    currentMode.FailCount = modeStats.FailCount;
                    currentMode.Scores = new List<Score>();
                    currentMode.JudgementCounts = new Dictionary<Judgement, int>
                    {
                        [Judgement.Marv] = modeStats.TotalMarvelous,
                        [Judgement.Perf] = modeStats.TotalPerfect,
                        [Judgement.Great] = modeStats.TotalGreat,
                        [Judgement.Good] = modeStats.TotalGood,
                        [Judgement.Okay] = modeStats.TotalOkay,
                        [Judgement.Miss] = modeStats.TotalMiss
                    };
                }
            }

            return this;
        }
    }
}
