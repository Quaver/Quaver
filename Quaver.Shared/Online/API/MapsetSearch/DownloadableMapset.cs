using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Quaver.API.Enums;

namespace Quaver.Shared.Online.API.MapsetSearch
{
    public class DownloadableMapset
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("creator_id")]
        public int CreatorId { get; set; }

        [JsonProperty("creator_username")]
        public string CreatorUsername { get; set; }

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

        [JsonProperty("maps")]
        public List<DownloadableMap> Maps { get; set; }

        /// <summary>
        ///     Whether or not we already have the set downloaded
        /// </summary>
        public bool IsOwned { get; set; }

        /// <summary>
        ///     The difficulty names + ratings in the set
        /// </summary>
        public Dictionary<string, double> Difficulties
        {
            get
            {
                var diffs = new Dictionary<string, double>();

                for (var i = 0; i < Maps.Count; i++)
                    diffs.Add(Maps[i].DifficultyName, Maps[i].DifficultyRating);

                return diffs.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }
        }
    }
}