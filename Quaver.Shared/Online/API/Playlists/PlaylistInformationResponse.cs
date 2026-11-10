using System.Collections.Generic;
using Newtonsoft.Json;
using Quaver.Shared.Online.API.Maps;

namespace Quaver.Shared.Online.API.Playlists
{
    public class PlaylistInformationResponse
    {
        [JsonProperty("status")]
        public int Status;

        [JsonProperty("playlist")]
        public PlaylistInformationResponsePlaylist PlaylistInformation;
    }

    public class PlaylistInformationResponsePlaylist
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("user")]
        public PlaylistInformationResponseUser User;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("map_count")]
        public int MapCount;

        [JsonProperty("timestamp")]
        public string TimeCreated;

        [JsonProperty("time_last_updated")]
        public string TimeLastUpdated;

        [JsonProperty("mapsets")]
        public List<PlaylistInformationResponseMapset> Mapsets;

        [JsonIgnore]
        public int OwnerId => User?.Id ?? 0;

        [JsonIgnore]
        public string OwnerUsername => User?.Username;
    }

    public class PlaylistInformationResponseUser
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("username")]
        public string Username;
    }

    public class PlaylistInformationResponseMapset
    {
        [JsonProperty("maps")]
        public List<PlaylistInformationResponseMap> Maps;
    }

    public class PlaylistInformationResponseMap
    {
        [JsonProperty("map")]
        public MapInformationResponseMap Map;
    }
}