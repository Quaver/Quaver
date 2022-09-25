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

        [JsonProperty("ranked_status")]
        public RankedStatus RankedStatus { get; set; }

        [JsonProperty("date_submitted")]
        public DateTime DateSubmitted { get; set; }

        [JsonProperty("date_last_updated")]
        public DateTime DateLastUpdated { get; set; }

        [JsonProperty("bpms")]
        public List<int> Bpms { get; set; }

        [JsonProperty("game_modes")]
        public List<GameMode> GameModes { get; set; }

        [JsonProperty("difficulty_names")]
        public List<string> DifficultyNames { get; set; }

        [JsonProperty("difficulty_range")]
        public List<double> DifficultyRange { get; set; }

        [JsonProperty("min_length_seconds")]
        public float MinSongLength { get; set; }

        [JsonProperty("max_length_seconds")]
        public float MaxLengthSeconds { get; set; }

        [JsonProperty("min_ln_percent")]
        public float MinLongNotePercent { get; set; }

        [JsonProperty("max_ln_percent")]
        public float MaxLongNotePercent { get; set; }

        [JsonProperty("min_play_count")]
        public int MinPlayCount { get; set; }

        [JsonProperty("max_play_count")]
        public int MaxPlayCount { get; set; }

        [JsonProperty("min_date_submitted")]
        public DateTime MinDateSubmitted { get; set; }

        [JsonProperty("max_date_submitted")]
        public DateTime MaxDateSubmitted { get; set; }

        [JsonProperty("min_date_last_updated")]
        public DateTime MinDateLastUpdated { get; set; }

        [JsonProperty("max_date_last_updated")]
        public DateTime MaxDateLastUpdated { get; set; }

        [JsonProperty("min_combo")]
        public int MinCombo { get; set; }

        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

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

                for (var i = 0; i < DifficultyNames.Count; i++)
                    diffs.Add(DifficultyNames[i], DifficultyRange[i]);

                return diffs.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }
        }
    }
}