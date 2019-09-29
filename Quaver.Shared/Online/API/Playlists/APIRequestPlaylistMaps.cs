using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quaver.Server.Client;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Database.Playlists;
using RestSharp;

namespace Quaver.Shared.Online.API.Playlists
{
    public class APIRequestPlaylistMaps : APIRequest<PlaylistMapsResponse>
    {
        /// <summary>
        /// </summary>
        public Playlist Playlist { get; }

        /// <summary>
        /// </summary>
        /// <param name="playlist"></param>
        public APIRequestPlaylistMaps(Playlist playlist) => Playlist = playlist;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override PlaylistMapsResponse ExecuteRequest()
        {
            var request = new RestRequest($"{APIEndpoint}playlist/{Playlist.OnlineMapPoolId}/maps", Method.GET);
            var client = new RestClient(OnlineClient.API_ENDPOINT) { UserAgent = "Quaver" };

            var response = client.Execute(request);

            var json = JObject.Parse(response.Content);

            var responseParsed = JsonConvert.DeserializeObject<PlaylistMapsResponse>(json.ToString());

            return responseParsed;
        }
    }
}