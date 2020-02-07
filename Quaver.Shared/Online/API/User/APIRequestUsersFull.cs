using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quaver.Server.Client;
using Quaver.Shared.Online.API.Playlists;
using RestSharp;

namespace Quaver.Shared.Online.API.User
{
    // ReSharper disable once InconsistentNaming
    public class APIRequestUsersFull : APIRequest<APIResponseUsersFull>
    {
        /// <summary>
        /// </summary>
        private int UserId { get; }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        public APIRequestUsersFull(int userId) => UserId = userId;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override APIResponseUsersFull ExecuteRequest()
        {
            var request = new RestRequest($"{APIEndpoint}users/full/{UserId}", Method.GET);
            var client = new RestClient(OnlineClient.API_ENDPOINT) { UserAgent = "Quaver" };

            var response = client.Execute(request);

            var json = JObject.Parse(response.Content);

            var responseParsed = JsonConvert.DeserializeObject<APIResponseUsersFull>(json.ToString());

            return responseParsed;
        }
    }
}