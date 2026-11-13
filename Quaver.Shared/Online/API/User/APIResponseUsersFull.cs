using Newtonsoft.Json;
using Quaver.API.Enums;

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
        [JsonProperty("accent_color_customizable")]
        public bool AccentColorCustomizable { get; set; }

        [JsonProperty("accent_color")]
        public string AccentColor { get; set; }

        [JsonProperty("misc_information")]
        public APIResponseUsersFullInformation MiscInformation { get; set; }

        [JsonProperty("stats_keys4")]
        public APIResponseUsersFullMode StatsKeys4 { get; set; }

        [JsonProperty("stats_keys7")]
        public APIResponseUsersFullMode StatsKeys7 { get; set; }
    }

    public class APIResponseUsersFullInformation
    {
        [JsonProperty("default_mode")]
        public GameMode DefaultMode { get; set; } = GameMode.Keys4;
    }

    // ReSharper disable once InconsistentNaming
    public class APIResponseUsersFullMode
    {
        [JsonProperty("ranks")]
        public APIResponseUsersFullRanks Ranks { get; set; }

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

        [JsonProperty("total_marvelous")]
        public int TotalMarvelous { get; set; }

        [JsonProperty("total_perfect")]
        public int TotalPerfect { get; set; }

        [JsonProperty("total_great")]
        public int TotalGreat { get; set; }

        [JsonProperty("total_good")]
        public int TotalGood { get; set; }

        [JsonProperty("total_okay")]
        public int TotalOkay { get; set; }

        [JsonProperty("total_miss")]
        public int TotalMiss { get; set; }
    }

    public class APIResponseUsersFullRanks
    {
        [JsonProperty("global")]
        public int Global { get; set; }

        [JsonProperty("country")]
        public int Country { get; set; }
    }
}