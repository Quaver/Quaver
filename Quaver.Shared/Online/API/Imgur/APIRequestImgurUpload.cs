using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Quaver.Server.Client;
using RestSharp;
using Wobble.Logging;

namespace Quaver.Shared.Online.API.Imgur
{
    public class APIRequestImgurUpload : APIRequest<string>
    {
        /// <summary>
        /// </summary>
        public const string ClientId  = "27ebbe781c4ce59";

        /// <summary>
        /// </summary>
        private string FilePath { get; }

        /// <summary>
        /// </summary>
        /// <param name="file"></param>
        public APIRequestImgurUpload(string file) => FilePath = file;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string ExecuteRequest()
        {
            try
            {
                var request = new RestRequest($"https://api.imgur.com/3/image", Method.POST);
                var client = new RestClient("https://api.imgur.com") { UserAgent = "Quaver" };

                request.AddHeader("Authorization", $"Client-ID {ClientId}");
                request.AddFile("image", FilePath);

                var response = client.Execute(request);
                var json = JObject.Parse(response.Content);

                if (!(bool) json["success"])
                    return null;

                return json["data"]["link"].ToString();
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Network);
                return null;
            }
        }
    }
}