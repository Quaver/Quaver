using System;
using System.Collections.Generic;
using Quaver.Server.Client.Structures;
using Steamworks;
using Wobble.Logging;

namespace Quaver.Shared.Online
{
    public class SteamAchievements : IDisposable
    {
        /// <summary>
        /// </summary>
        private List<ScoreSubmissionResponseAchievement> Achievements { get; }

        /// <summary>
        ///     The callback that will be ran when the client receives updated user stats
        /// </summary>
        private static Callback<UserStatsReceived_t> UserStatsReceivedCallback { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="achievements"></param>
        public SteamAchievements(List<ScoreSubmissionResponseAchievement> achievements)
        {
            Achievements = achievements;
            UserStatsReceivedCallback = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        }

        /// <summary>
        ///     Unlocks or updates each achievement
        /// </summary>
        public void Unlock() => SteamUserStats.RequestCurrentStats();

        /// <summary>
        ///     In order to unlock achievements, a request for updated user stats must be called
        ///     So we'll handle the actual unlock process here.
        /// </summary>
        /// <param name="param"></param>
        private void OnUserStatsReceived(UserStatsReceived_t param)
        {
            foreach (var achievement in Achievements)
            {
                Logger.Important($"Achievement Progress Received - {achievement.Id} | {achievement.SteamApiName}", LogType.Network);
                SteamUserStats.SetAchievement(achievement.SteamApiName);
                SteamUserStats.StoreStats();
            }

            Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose() => UserStatsReceivedCallback.Dispose();
    }
}