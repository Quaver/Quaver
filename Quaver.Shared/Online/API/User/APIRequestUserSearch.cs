using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Quaver.Server.Client;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using RestSharp;
using OnlineUserModel = Quaver.Server.Client.Structures.User;

namespace Quaver.Shared.Online.API.User
{
    // ReSharper disable once InconsistentNaming
    public class APIRequestUserSearch : APIRequest<List<OnlineUserModel>>
    {
        private const int MaxResults = 50;

        private string Query { get; }

        public APIRequestUserSearch(string query) => Query = query;

        public override List<OnlineUserModel> ExecuteRequest()
        {
            if (string.IsNullOrWhiteSpace(Query))
                return new List<OnlineUserModel>();

            var query = Uri.EscapeDataString(Query.Trim());
            var request = new RestRequest($"{OnlineClient.API_ENDPOINT}/v2/user/search/{query}", Method.GET);
            var client = new RestClient(OnlineClient.API_ENDPOINT) { UserAgent = "Quaver" };
            var response = ExecuteApiRequest(client, request);

            if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
                return new List<OnlineUserModel>();

            var result = JsonConvert.DeserializeObject<APIResponseUserSearch>(response.Content);
            return (result?.Users ?? new List<APIResponseUserSearchUser>())
                .Take(MaxResults)
                .Select(user => user.ToUser())
                .ToList();
        }
    }

    public class APIResponseUserSearch
    {
        [JsonProperty("users")]
        public List<APIResponseUserSearchUser> Users { get; set; }
    }

    public class APIResponseUserSearchUser
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("steam_id")]
        public string SteamId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("usergroups")]
        public UserGroups UserGroups { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("clan_id")]
        public string ClanId { get; set; }

        [JsonProperty("clan_tag")]
        public string ClanTag { get; set; }

        [JsonProperty("clan_accent_color")]
        public string ClanAccentColor { get; set; }

        [JsonProperty("latest_activity")]
        public DateTime LatestActivity { get; set; }

        public OnlineUserModel ToUser()
        {
            long.TryParse(SteamId, out var steamId);

            return new APIUserSearchResult(new OnlineUser
            {
                Id = Id,
                SteamId = steamId,
                Username = Username,
                UserGroups = UserGroups,
                CountryFlag = string.IsNullOrEmpty(Country) ? "XX" : Country,
                ClanId = ClanId,
                ClanTag = ClanTag,
                ClanAccentColor = ClanAccentColor
            }, LatestActivity);
        }
    }

    public sealed class APIUserSearchResult : OnlineUserModel
    {
        public DateTime LatestActivity { get; }

        public APIUserSearchResult(OnlineUser user, DateTime latestActivity) : base(user) =>
            LatestActivity = latestActivity;
    }
}
