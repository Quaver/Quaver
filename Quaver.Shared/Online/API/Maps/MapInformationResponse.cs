using Newtonsoft.Json;
using Quaver.API.Enums;

namespace Quaver.Shared.Online.API.Maps
{
    public class MapInformationResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("map")]
        public MapInformationResponseMap Map { get; set; }
    }

    public class MapInformationResponseMap
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("mapset_id")]
        public int MapsetId { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("alternative_md5")]
        public string AlternativeMd5 { get; set; }

        [JsonProperty("creator_id")]
        public int CreatorId { get; set; }

        [JsonProperty("creator_username")]
        public string CreatorUsername { get; set; }

        [JsonProperty("game_mode")]
        public GameMode GameMode { get; set; }

        [JsonProperty("ranked_status")]
        public RankedStatus RankedStatus { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("difficulty_name")]
        public string DifficultyName { get; set; }
    }
}