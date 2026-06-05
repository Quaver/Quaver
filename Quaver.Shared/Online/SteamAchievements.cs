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
        ///     The call result that will be ran when the client receives updated user stats
        /// </summary>
        private CallResult<UserStatsReceived_t>? UserStatsReceivedCallResult { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="achievements"></param>
        public SteamAchievements(List<ScoreSubmissionResponseAchievement> achievements)
        {
            Achievements = achievements;
            UserStatsReceivedCallResult = CallResult<UserStatsReceived_t>.Create(OnUserStatsReceived);
        }

        /// <summary>
        ///     Unlocks or updates each achievement
        /// </summary>
        public void Unlock()
        {
            var call = SteamUserStats.RequestUserStats(SteamUser.GetSteamID());
            UserStatsReceivedCallResult?.Set(call);
        }

        /// <summary>
        ///     In order to unlock achievements, a request for updated user stats must be called
        ///     So we'll handle the actual unlock process here.
        /// </summary>
        /// <param name="param"></param>
        private void OnUserStatsReceived(UserStatsReceived_t param, bool bIOfailure)
        {
            if (bIOfailure)
            {
                Logger.Error("Failed to receive Steam user stats before unlocking achievements", LogType.Network);
                Dispose();
                return;
            }

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
        public void Dispose()
        {
            UserStatsReceivedCallResult?.Dispose();
            UserStatsReceivedCallResult = null;
        }
    }
}
