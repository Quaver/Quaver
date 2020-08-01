using System.Collections.Generic;
using Newtonsoft.Json;

namespace Quaver.Shared.Online.API.Ranked
{
    public class RankedMapsetsResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("mapsets")]
        public List<int> Mapsets { get; set; }
    }
}