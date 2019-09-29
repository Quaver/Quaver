using System.Collections.Generic;
using Newtonsoft.Json;

namespace Quaver.Shared.Online.API.Playlists
{
    public class PlaylistMapsResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("map_ids")]
        public List<int> MapIds { get; set; }
    }
}