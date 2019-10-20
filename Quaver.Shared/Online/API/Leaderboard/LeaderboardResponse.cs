using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Quaver.Server.Common.Enums;

namespace Quaver.Shared.Online.API.Leaderboard
{
    public class LeaderboardResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("users")]
        public List<LeaderboardUser> Users { get; set; }
    }

    public class LeaderboardUser
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("steam_id")]
        public long SteamId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("allowed")]
        public bool Allowed { get; set; }

        [JsonProperty("privileges")]
        public Privileges Privileges { get; set; }

        [JsonProperty("usergroups")]
        public UserGroups UserGroups { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("time_registered")]
        public DateTime TimeRegistered { get; set; }

        [JsonProperty("latest_activity")]
        public DateTime LatestActivity { get; set; }
    }
}