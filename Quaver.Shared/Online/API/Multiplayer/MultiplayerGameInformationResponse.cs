using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Quaver.API.Enums;
using Quaver.Server.Client.Objects.Multiplayer;

namespace Quaver.Shared.Online.API.Multiplayer;

public class MultiplayerGameInformationResponse
{
    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("game")]
    public MultiplayerGameInformationResponseGame Game { get; set; }
}

public class MultiplayerGameInformationResponseGame
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("unique_id")]
    public string UniqueId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("type")]
    public int Type { get; set; }

    [JsonProperty("time_created")]
    public DateTime TimeCreated { get; set; }

    [JsonProperty("matches")]
    public List<MultiplayerGameInformationResponseMatch> Matches { get; set; }
}


public class MultiplayerGameInformationResponseMatch
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("game_id")]
    public int GameId { get; set; }

    [JsonProperty("time_played")]
    public DateTime TimePlayed { get; set; }

    [JsonProperty("map_md5")]
    public string MapMd5 { get; set; }

    [JsonProperty("map_string")]
    public string MapString { get; set; }

    [JsonProperty("host_id")]
    public int HostId { get; set; }

    [JsonProperty("game_mode")]
    public GameMode GameMode { get; set; }

    [JsonProperty("global_modifiers")]
    public ModIdentifier GlobalModifiers { get; set; }

    [JsonProperty("free_mod_type")]
    public int FreeModType { get; set; }

    [JsonProperty("aborted")]
    public bool Aborted { get; set; }

    [JsonProperty("aborted_early")]
    public bool AbortedEarly { get; set; }

    [JsonProperty("outcome")]
    public MultiplayerGameInformationResponseOutcome Outcome { get; set; }

    [JsonProperty("rules")]
    public MultiplayerResponseRules Rules { get; set; }

    [JsonProperty("most_valuable_player")]
    public MultiplayerGameInformationResponsePlayer MostValuablePlayer { get; set; }

    [JsonProperty("map")]
    public MultiplayerGameInformationResponseMap Map { get; set; }

    [JsonProperty("scores")]
    public List<MultiplayerGameInformationResponseScore> Scores { get; set; }
}

public class MultiplayerGameInformationResponseOutcome
{
    [JsonProperty("result")]
    public int Result { get; set; }

    [JsonProperty("team")]
    public int Team { get; set; }
}

public class MultiplayerResponseRules
{
    [JsonProperty("ruleset")]
    public MultiplayerGameRuleset Ruleset { get; set; }

    [JsonProperty("mods")]
    public ModIdentifier Mods { get; set; }

    [JsonProperty("mods_string")]
    public string ModsString { get; set; }

    [JsonProperty("free_mod_type")]
    public int FreeModType { get; set; }

    [JsonProperty("health_type")]
    public int HealthType { get; set; }

    [JsonProperty("lives")]
    public int Lives { get; set; }
}

public class MultiplayerGameInformationResponseMap
{
    [JsonProperty("id")]
    public int? Id { get; set; }

    [JsonProperty("mapset_id")]
    public int? MapsetId { get; set; }

    [JsonProperty("md5")]
    public string Md5 { get; set; }

    [JsonProperty("game_mode")]
    public GameMode GameMode { get; set; }

    [JsonProperty("ranked_status")]
    public RankedStatus RankedStatus { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}

public class MultiplayerGameInformationResponsePlayer
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }

    [JsonProperty("avatar_url")]
    public string AvatarUrl { get; set; }

    [JsonProperty("score")]
    public MultiplayerGameInformationResponseScore Score { get; set; }
}

public class MultiplayerGameInformationResponseScore
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("user_id")]
    public int UserId { get; set; }

    [JsonProperty("match_id")]
    public int MatchId { get; set; }

    [JsonProperty("modifiers")]
    public ModIdentifier Modifiers { get; set; }

    [JsonProperty("team")]
    public int Team { get; set; }

    [JsonProperty("performance_rating")]
    public double PerformanceRating { get; set; }

    [JsonProperty("accuracy")]
    public double Accuracy { get; set; }

    [JsonProperty("max_combo")]
    public int MaxCombo { get; set; }

    [JsonProperty("count_marvelous")]
    public int CountMarvelous { get; set; }

    [JsonProperty("count_perfect")]
    public int CountPerfect { get; set; }

    [JsonProperty("count_great")]
    public int CountGreat { get; set; }

    [JsonProperty("count_good")]
    public int CountGood { get; set; }

    [JsonProperty("count_okay")]
    public int CountOkay { get; set; }

    [JsonProperty("count_miss")]
    public int CountMiss { get; set; }

    [JsonProperty("count_minehit")]
    public int CountMineHit { get; set; }

    [JsonProperty("won")]
    public int Won { get; set; }

    [JsonProperty("user")]
    public MultiplayerGameInformationResponsePlayer User { get; set; }
}
