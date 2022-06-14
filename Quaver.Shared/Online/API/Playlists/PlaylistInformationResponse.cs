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

        [JsonProperty("user_id")]
        public int UserId;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("like_count")]
        public int LikeCount;

        [JsonProperty("map_count")]
        public int MapCount;

        [JsonProperty("time_created")]
        public string TimeCreated;

        [JsonProperty("time_last_updated")]
        public string TimeLastUpdated;

        [JsonProperty("owner_id")]
        public int OwnerId;

        [JsonProperty("owner_username")]
        public string OwnerUsername;

        [JsonProperty("maps")]
        public List<MapInformationResponseMap> Maps;

    }
}