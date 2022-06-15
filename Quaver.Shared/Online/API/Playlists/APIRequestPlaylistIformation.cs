using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quaver.Server.Client;
using RestSharp;

namespace Quaver.Shared.Online.API.Playlists
{
    public class APIRequestPlaylistInformation : APIRequest<PlaylistInformationResponse>
    {
        /// <summary>
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        public APIRequestPlaylistInformation(int id) => Id = id;

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override PlaylistInformationResponse ExecuteRequest()
        {
            var request = new RestRequest($"{APIEndpoint}playlist/{Id}", Method.GET);
            var client = new RestClient(OnlineClient.API_ENDPOINT) { UserAgent = "Quaver" };

            var response = client.Execute(request);

            var json = JObject.Parse(response.Content);

            var responseParsed = JsonConvert.DeserializeObject<PlaylistInformationResponse>(json.ToString());

            return responseParsed;
        }
    }
}