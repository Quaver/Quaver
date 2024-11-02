using Newtonsoft.Json;
using Quaver.API.Enums;

namespace Quaver.Shared.Online.API.MapsetSearch;

public class DownloadableMap
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("mapset_id")]
    public int MapsetId { get; set; }

    [JsonProperty("md5")]
    public string Md5 { get; set; }

    [JsonProperty("alternative_md5")]
    public string AlternativeMd5 { get; set; }

    [JsonProperty("creator_id")]
    public int CreatorId { get; set; }

    [JsonProperty("creator_username")]
    public string CreatorUsername { get; set; }

    [JsonProperty("game_mode")]
    public GameMode GameMode { get; set; }

    [JsonProperty("ranked_status")]
    public RankedStatus RankedStatus { get; set; }

    [JsonProperty("artist")]
    public string Artist { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("source")]
    public string Source { get; set; }

    [JsonProperty("tags")]
    public string Tags { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("difficulty_name")]
    public string DifficultyName { get; set; }

    [JsonProperty("length")]
    public int Length { get; set; }

    [JsonProperty("bpm")]
    public float Bpm { get; set; }

    [JsonProperty("difficulty_rating")]
    public double DifficultyRating { get; set; }

    [JsonProperty("count_hitobject_normal")]
    public int CountHitObjectNormal { get; set; }

    [JsonProperty("count_hitobject_long")]
    public int CountHitObjectLong { get; set; }

    [JsonProperty("long_note_percentage")]
    public double LongNotePercentage { get; set; }

    [JsonProperty("max_combo")]
    public int MaxCombo { get; set; }

    [JsonProperty("play_count")]
    public int PlayCount { get; set; }

    [JsonProperty("fail_count")]
    public int FailCount { get; set; }

    [JsonProperty("play_attempts")]
    public int PlayAttempts { get; set; }

    [JsonProperty("online_offset")]
    public int OnlineOffset { get; set; }

    [JsonProperty("is_clan_ranked")]
    public bool IsClanRanked { get; set; }
}