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
        ///     Called when the pending Steam user stats call has completed.
        /// </summary>
        private Action<SteamAchievements> OnCompleted { get; }

        /// <summary>
        ///     The call result that will be ran when the client receives updated user stats
        /// </summary>
        private CallResult<UserStatsReceived_t>? UserStatsReceivedCallResult { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="achievements"></param>
        /// <param name="onCompleted"></param>
        public SteamAchievements(List<ScoreSubmissionResponseAchievement> achievements, Action<SteamAchievements> onCompleted)
        {
            Achievements = achievements;
            OnCompleted = onCompleted;
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
                Complete();
                return;
            }

            foreach (var achievement in Achievements)
            {
                Logger.Important($"Achievement Progress Received - {achievement.Id} | {achievement.SteamApiName}", LogType.Network);
                SteamUserStats.SetAchievement(achievement.SteamApiName);
                SteamUserStats.StoreStats();
            }

            Complete();
        }

        /// <summary>
        ///     Disposes the call result and tells the owner this pending call no longer needs to be rooted.
        /// </summary>
        private void Complete()
        {
            Dispose();
            OnCompleted(this);
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
