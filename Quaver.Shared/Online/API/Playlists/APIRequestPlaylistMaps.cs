using System;
using System.Collections.Generic;
using System.Linq;
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
            var client = new RestClient(OnlineClient.API_ENDPOINT) { UserAgent = "Quaver" };
            var mapIds = new HashSet<int>();
            const int pageSize = 25;
            var page = 0;

            while (true)
            {
                var request = new RestRequest($"{APIEndpoint}playlists/{Playlist.OnlineMapPoolId}?page={page}&limit={pageSize}", Method.GET);
                var response = ExecuteApiRequest(client, request);
                var status = (int) response.StatusCode;

                if (status != 200)
                    return new PlaylistMapsResponse { Status = status, MapIds = mapIds.ToList() };

                var json = JObject.Parse(response.Content);
                var responseParsed = JsonConvert.DeserializeObject<PlaylistInformationResponse>(json.ToString());
                var mapsets = responseParsed.PlaylistInformation?.Mapsets ?? new List<PlaylistInformationResponseMapset>();

                foreach (var map in mapsets.SelectMany(x => x.Maps ?? new List<PlaylistInformationResponseMap>()))
                {
                    if (map.Map != null)
                        mapIds.Add(map.Map.Id);
                }

                if (mapsets.Count < pageSize || mapIds.Count >= responseParsed.PlaylistInformation.MapCount)
                    return new PlaylistMapsResponse { Status = status, MapIds = mapIds.ToList() };

                page++;
            }
        }
    }
}
