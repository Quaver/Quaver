using System.Collections.Generic;
using Newtonsoft.Json;

namespace Quaver.Shared.Online.API.Offsets
{
    public class OnlineOffsetResponse
    {
        // v1-only
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("online_offsets")]
        public List<OnlineOffsetMap> Maps { get; set; }
    }

    public class OnlineOffsetMap
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }
    }
}