using Newtonsoft.Json;

namespace Quaver.Shared.Online.API.User
{
    // ReSharper disable once InconsistentNaming
    public class APIResponseUsersFull
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("user")]
        public APIResponseUsersFullUser User { get; set; }
    }

    // ReSharper disable once InconsistentNaming
    public class APIResponseUsersFullUser
    {
        [JsonProperty("keys4")]
        public APIResponseUsersFullMode Keys4 { get; set; }

        [JsonProperty("keys7")]
        public APIResponseUsersFullMode Keys7 { get; set; }
    }

    // ReSharper disable once InconsistentNaming
    public class APIResponseUsersFullMode
    {
        [JsonProperty("globalRank")]
        public int GlobalRank { get; set; }

        [JsonProperty("countryRank")]
        public int CountryRank { get; set; }

        [JsonProperty("multiplayerWinRank")]
        public int MultiplayerWinsRank { get; set; }

        [JsonProperty("stats")]
        public APIRequestUsersFullStats Stats { get; set; }
    }
    
    // ReSharper disable once InconsistentNaming
    public class APIRequestUsersFullStats
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("total_score")]
        public long TotalScore { get; set; }

        [JsonProperty("ranked_score")]
        public long RankedScore { get; set; }

        [JsonProperty("overall_accuracy")]
        public double OverallAccuracy { get; set; }

        [JsonProperty("overall_performance_rating")]
        public double OverallPerformanceRating { get; set; }
        
        [JsonProperty("play_count")]
        public int PlayCount { get; set; }

        [JsonProperty("fail_count")]
        public int FailCount { get; set; }

        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty("total_marv")]
        public int TotalMarv { get; set; }
        
        [JsonProperty("total_perf")]
        public int TotalPerf { get; set; }
        
        [JsonProperty("total_great")]
        public int TotalGreat { get; set; }
        
        [JsonProperty("total_good")]
        public int TotalGood { get; set; }
        
        [JsonProperty("total_okay")]
        public int TotalOkay { get; set; }
        
        [JsonProperty("total_miss")]
        public int TotalMiss { get; set; }
    }
}