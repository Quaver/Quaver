using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quaver.Server.Client;
using Quaver.Shared.Online.API.Playlists;
using RestSharp;

namespace Quaver.Shared.Online.API.Maps
{
    public class APIRequestMapInformation : APIRequest<MapInformationResponse>
    {
        /// <summary>
        ///     The id of the map to lookup
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        public APIRequestMapInformation(int id) => Id = id;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override MapInformationResponse ExecuteRequest()
        {
            var request = new RestRequest($"{APIEndpoint}maps/{Id}", Method.GET);
            var client = new RestClient(OnlineClient.API_ENDPOINT) { UserAgent = "Quaver" };

            var response = client.Execute(request);

            var json = JObject.Parse(response.Content);

            var responseParsed = JsonConvert.DeserializeObject<MapInformationResponse>(json.ToString());

            return responseParsed;
        }
    }
}