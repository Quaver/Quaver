using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Quaver.API.Enums;

namespace Quaver.Shared.Online.API.Multiplayer;

public class MultiplayerMatchInformationResponseMap
{
    [JsonProperty("id")] public int? Id { get; set; }

    [JsonProperty("mapset_id")] public int? MapsetId { get; set; }

    [JsonProperty("md5")] public string Md5 { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("game_mode")] public GameMode GameMode { get; set; }
}

public class MultiplayerMatchInformationResponsePlayer
{
    [JsonProperty("id")] public int Id { get; set; }

    [JsonProperty("username")] public string Username { get; set; }

    [JsonProperty("country")] public string Country { get; set; }

    [JsonProperty("avatar_url")] public string AvatarUrl { get; set; }
}

public class MultiplayerMatchInformationResponseScore
{
    [JsonProperty("team")] public int Team { get; set; }

    [JsonProperty("win_result")] public int WinResult { get; set; }

    [JsonProperty("has_failed")] public bool HasFailed { get; set; }

    [JsonProperty("mods")] public ModIdentifier Mods { get; set; }

    [JsonProperty("mods_string")] public string ModsString { get; set; }

    [JsonProperty("full_combo")] public bool FullCombo { get; set; }

    [JsonProperty("lives_left")] public int LivesLeft { get; set; }

    [JsonProperty("performance_rating")] public double PerformanceRating { get; set; }

    [JsonProperty("accuracy")] public double Accuracy { get; set; }

    [JsonProperty("score")] public int Score { get; set; }

    [JsonProperty("grade")] public string Grade { get; set; }

    [JsonProperty("max_combo")] public int MaxCombo { get; set; }

    [JsonProperty("count_marv")] public int CountMarv { get; set; }

    [JsonProperty("count_perf")] public int CountPerf { get; set; }

    [JsonProperty("count_great")] public int CountGreat { get; set; }

    [JsonProperty("count_good")] public int CountGood { get; set; }

    [JsonProperty("count_okay")] public int CountOkay { get; set; }

    [JsonProperty("count_miss")] public int CountMiss { get; set; }

    [JsonProperty("battle_royale_rank")] public int? BattleRoyaleRank { get; set; }
}

public class MultiplayerMatchInformationResponsePlayerScore
{
    [JsonProperty("player")] public MultiplayerMatchInformationResponsePlayer Player { get; set; }

    [JsonProperty("score")] public MultiplayerMatchInformationResponseScore Score { get; set; }
}

public class MultiplayerMatchInformationResponseMatch
{
    [JsonProperty("id")] public int Id { get; set; }

    [JsonProperty("time_played")] public DateTime TimePlayed { get; set; }

    [JsonProperty("aborted_early")] public bool AbortedEarly { get; set; }

    [JsonProperty("outcome")] public MultiplayerGameInformationResponseOutcome Outcome { get; set; }

    [JsonProperty("rules")] public MultiplayerResponseRules Rules { get; set; }

    [JsonProperty("map")] public MultiplayerMatchInformationResponseMap Map { get; set; }

    [JsonProperty("scores")] public List<MultiplayerMatchInformationResponsePlayerScore> Scores { get; set; }
}

public class MultiplayerMatchInformationResponse
{
    [JsonProperty("status")] public int Status { get; set; }

    [JsonProperty("game")] public MultiplayerGameInformationResponseGame Game { get; set; }

    [JsonProperty("match")] public MultiplayerMatchInformationResponseMatch Match { get; set; }
}