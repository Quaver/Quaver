using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quaver.API.Enums;
using Quaver.Server.Client;
using Quaver.Shared.Online.API.Maps;
using RestSharp;

namespace Quaver.Shared.Online.API.Leaderboard
{
    public class APIRequestLeaderboard : APIRequest<LeaderboardResponse>
    {
        private GameMode Mode { get; }

        public APIRequestLeaderboard(GameMode mode) => Mode = mode;

        public override LeaderboardResponse ExecuteRequest()
        {
            var request = new RestRequest($"{APIEndpoint}leaderboard?mode={(int) Mode}", Method.GET);
            var client = new RestClient(OnlineClient.API_ENDPOINT) { UserAgent = "Quaver" };

            var response = client.Execute(request);

            var json = JObject.Parse(response.Content);

            var responseParsed = JsonConvert.DeserializeObject<LeaderboardResponse>(json.ToString());

            return responseParsed;
        }
    }
}