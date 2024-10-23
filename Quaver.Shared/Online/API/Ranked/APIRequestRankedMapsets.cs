using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quaver.Server.Client;
using Quaver.Shared.Online.API.Playlists;
using RestSharp;

namespace Quaver.Shared.Online.API.Ranked
{
    public class APIRequestRankedMapsets : APIRequest<RankedMapsetsResponse>
    {
        public override RankedMapsetsResponse ExecuteRequest()
        {
            var request = new RestRequest($"{OnlineClient.API_ENDPOINT}/v2/mapset/ranked", Method.GET);
            var client = new RestClient(OnlineClient.API_ENDPOINT) { UserAgent = "Quaver" };

            var response = client.Execute(request);

            var json = JObject.Parse(response.Content);

            var responseParsed = JsonConvert.DeserializeObject<RankedMapsetsResponse>(json.ToString());

            return responseParsed;
        }
    }
}