using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quaver.Server.Client;
using Quaver.Shared.Online.API.Ranked;
using RestSharp;

namespace Quaver.Shared.Online.API.Offsets
{
    public class APIRequestOnlineOffsets : APIRequest<OnlineOffsetResponse>
    {
        public override OnlineOffsetResponse ExecuteRequest()
        {
            var request = new RestRequest($"{OnlineClient.API_ENDPOINT}/v2/mapset/offsets", Method.GET);
            var client = new RestClient(OnlineClient.API_ENDPOINT) { UserAgent = "Quaver" };

            var response = client.Execute(request);

            var json = JObject.Parse(response.Content);

            var responseParsed = JsonConvert.DeserializeObject<OnlineOffsetResponse>(json.ToString());

            return responseParsed;
        }
    }
}