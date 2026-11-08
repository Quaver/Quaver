using Newtonsoft.Json;
using Quaver.Server.Client;
using RestSharp;

namespace Quaver.Shared.Online.API.User
{
    // ReSharper disable once InconsistentNaming
    public sealed class APIRequestUserProfile : APIRequest<APIResponseUserProfile>
    {
        private int UserId { get; }

        public APIRequestUserProfile(int userId) => UserId = userId;

        public override APIResponseUserProfile ExecuteRequest()
        {
            var request = new RestRequest($"{OnlineClient.API_ENDPOINT}/v2/user/{UserId}", Method.GET);
            var client = new RestClient(OnlineClient.API_ENDPOINT) { UserAgent = "Quaver" };
            var response = ExecuteApiRequest(client, request);
            return JsonConvert.DeserializeObject<APIResponseUserProfile>(response.Content);
        }
    }

    // ReSharper disable once InconsistentNaming
    public sealed class APIResponseUserProfile
    {
        [JsonProperty("user")]
        public APIResponseUserProfileUser User { get; set; }
    }

    // ReSharper disable once InconsistentNaming
    public sealed class APIResponseUserProfileUser
    {
        [JsonProperty("accent_color_customizable")]
        public bool AccentColorCustomizable { get; set; }

        [JsonProperty("accent_color")]
        public string AccentColor { get; set; }
    }
}
