using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Quaver.Server.Client;
using RestSharp;
using OnlineUserModel = Quaver.Server.Client.Structures.User;

namespace Quaver.Shared.Online.API.User
{
    // ReSharper disable once InconsistentNaming
    public class APIRequestClanMembers : APIRequest<List<OnlineUserModel>>
    {
        private int ClanId { get; }

        public APIRequestClanMembers(int clanId) => ClanId = clanId;

        public override List<OnlineUserModel> ExecuteRequest()
        {
            var request = new RestRequest($"{OnlineClient.API_ENDPOINT}/v2/clan/{ClanId}/members", Method.GET);
            var client = new RestClient(OnlineClient.API_ENDPOINT) { UserAgent = "Quaver" };
            var response = ExecuteApiRequest(client, request);

            if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
                return new List<OnlineUserModel>();

            var result = JsonConvert.DeserializeObject<APIResponseClanMembers>(response.Content);
            return (result?.ClanMembers ?? new List<APIResponseUserSearchUser>())
                .Select(user => user.ToUser())
                .ToList();
        }
    }

    public class APIResponseClanMembers
    {
        [JsonProperty("clan_members")]
        public List<APIResponseUserSearchUser> ClanMembers { get; set; }
    }
}
