using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Quaver.Shared.Online.API.News
{
    public class APIResponseNewsFeed
    {
        [JsonProperty("code")]
        public int Code { get; set;}

        [JsonProperty("items")]
        public List<NewsFeedItem> Items { get; set; }
        
        [JsonIgnore]
        public Texture2D RecentPostBanner { get; set; }
    }

    public class NewsFeedItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }
        
        [JsonProperty("short_text")]
        public string ShortText { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("ingame_thumbnail")]
        public string IngameThumbnail { get; set; }

        [JsonProperty("date_published")]
        public DateTime DatePublished { get; set; }

        [JsonProperty("date_modified")]
        public DateTime DateModified { get; set; }
    }
}