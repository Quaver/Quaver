using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Quaver.API.Enums;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Screens.Gameplay.Rulesets;

namespace Quaver.Shared.Online.API.Multiplayer;

public class MultiplayerGameInformationResponse
{
    [JsonProperty("status")]
    public int Status { get; set; }
    
    [JsonProperty("multiplayer_game")]
    public MultiplayerGameInformationResponseGame MultiplayerGame { get; set; }
    
    [JsonProperty("matches")]
    public List<MultiplayerGameInformationResponseMatch> Matches { get; set; }
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
}


public class MultiplayerGameInformationResponseMatch
{
    [JsonProperty("id")]
    public int Id { get; set; }
    
    [JsonProperty("time_played")]
    public DateTime TimePlayed { get; set; }
    
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
    [JsonProperty("team")]
    public int Team { get; set; }
    
    [JsonProperty("performance_rating")]
    public double PerformanceRating { get; set; }
    
    [JsonProperty("accuracy")]
    public double Accuracy { get; set; }
}