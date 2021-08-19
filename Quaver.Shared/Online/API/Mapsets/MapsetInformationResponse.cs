using System.Collections.Generic;
using Newtonsoft.Json;
using Quaver.API.Enums;
using Quaver.Shared.Online.API.Maps;

namespace Quaver.Shared.Online.API.Mapsets
{
    public class MapsetInformationResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("mapset")]
        public MapsetInformationResponseMap Mapset { get; set; }
    }

    public class MapsetInformationResponseMap
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("creator_id")]
        public int CreatorId { get; set; }

        [JsonProperty("creator_username")]
        public string CreatorUsername { get; set; }

        [JsonProperty("creator_avatar_url")]
        public string CreatorAvatarUrl { get; set; }

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

        [JsonProperty("date_submitted")]
        public string DateSubmitted { get; set; }

        [JsonProperty("date_last_updated")]
        public string DateLastUpdated { get; set; }

        [JsonProperty("ranking_queue_status")]
        public int? RankingQueueStatus { get; set; }

        [JsonProperty("ranking_queue_last_updated")]
        public string RankingQueueLastUpdated { get; set; }

        [JsonProperty("ranking_queue_vote_count")]
        public int? RankingQueueVoteCount { get; set; }

        [JsonProperty("mapset_ranking_queue_id")]
        public int? MapsetRankingQueueId { get; set; }

        [JsonProperty("maps")]
        public List<MapInformationResponseMap> Maps { get; set; }
    }
}