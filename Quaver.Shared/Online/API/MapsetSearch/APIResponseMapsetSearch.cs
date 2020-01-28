using System.Collections.Generic;
using Newtonsoft.Json;

namespace Quaver.Shared.Online.API.MapsetSearch
{
    public class APIResponseMapsetSearch
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("mapsets")]
        public List<DownloadableMapset> Mapsets { get; set; }
    }
}