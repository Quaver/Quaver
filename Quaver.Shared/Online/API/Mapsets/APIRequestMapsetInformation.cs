using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quaver.Server.Client;
using RestSharp;

namespace Quaver.Shared.Online.API.Mapsets
{
    public class APIRequestMapsetInformation : APIRequest<MapsetInformationResponse>
    {
        /// <summary>
        ///     The id of the map to lookup
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        public APIRequestMapsetInformation(int id) => Id = id;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override MapsetInformationResponse ExecuteRequest()
        {
            var request = new RestRequest($"{APIEndpoint}mapsets/{Id}", Method.GET);
            var client = new RestClient(OnlineClient.API_ENDPOINT) { UserAgent = "Quaver" };

            var response = client.Execute(request);

            var json = JObject.Parse(response.Content);

            var responseParsed = JsonConvert.DeserializeObject<MapsetInformationResponse>(json.ToString());

            return responseParsed;
        }
    }
}