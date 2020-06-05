using System;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quaver.Server.Client;
using Quaver.Shared.Online.API.Leaderboard;
using Quaver.Shared.Screens.Download;
using RestSharp;
using Wobble.Assets;
using Wobble.Logging;

namespace Quaver.Shared.Online.API.News
{
    public class APIRequestNewsFeed : APIRequest<APIResponseNewsFeed>
    {
        private const string URL = "https://blog.quavergame.com/feed.json";
        
        public override APIResponseNewsFeed ExecuteRequest()
        {
            var request = new RestRequest(URL, Method.GET);
            var client = new RestClient("https://blog.quavergame.com") { UserAgent = "Quaver" };

            var response = client.Execute(request);

            var json = JObject.Parse(response.Content);

            var responseParsed = JsonConvert.DeserializeObject<APIResponseNewsFeed>(json.ToString());

            LoadBanner(responseParsed);
            
            return responseParsed;
        }

        /// <summary>
        ///     Loads the banner 
        /// </summary>
        private void LoadBanner(APIResponseNewsFeed feed)
        {
            if (feed.Items.Count == 0)
            {
                Logger.Warning($"Could not load most recent news feed banner due to no blog items", LogType.Runtime);
                return;
            }

            var latestPost = feed.Items.First();
            
            try
            {
                using (var webClient = new WebClient())
                {
                    using (var mem = new MemoryStream(webClient.DownloadData(latestPost.Image)))
                        feed.RecentPostBanner = AssetLoader.LoadTexture2D(mem);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }
    }
}