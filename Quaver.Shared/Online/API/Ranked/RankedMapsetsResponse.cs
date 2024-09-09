using System.Collections.Generic;
using Newtonsoft.Json;

namespace Quaver.Shared.Online.API.Ranked
{
    public class RankedMapsetsResponse
    {
        // v1-only
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("ranked_mapsets")]
        public List<int> Mapsets { get; set; }
    }
}