using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quaver.API.Enums;
using Quaver.Server.Client;
using Quaver.Shared.Screens.Downloading.UI.Search;
using RestSharp;
using Wobble.Logging;

namespace Quaver.Shared.Online.API.MapsetSearch
{
    // ReSharper disable once InconsistentNaming
    public class APIRequestMapsetSearch : APIRequest<APIResponseMapsetSearch>
    {
        /// <summary>
        /// </summary>
        private string Query { get; }

        /// <summary>
        /// </summary>
        private DownloadFilterMode Mode { get; }

        /// <summary>
        /// </summary>
        private DownloadFilterRankedStatus Status { get; }

        /// <summary>
        /// </summary>
        private float MinDiffifculty { get; }

        /// <summary>
        /// </summary>
        private float MaxDifficulty { get; }

        /// <summary>
        /// </summary>
        private float MinBpm { get; }

        /// <summary>
        /// </summary>
        private float MaxBpm { get; }

        /// <summary>
        /// </summary>
        private int MinLength { get; }

        /// <summary>
        /// </summary>
        private int MaxLength { get; }

        /// <summary>
        /// </summary>
        private int MinLongNotePercent { get; }

        /// <summary>
        /// </summary>
        private int MaxLongNotePercent { get; }

        /// <summary>
        /// </summary>
        private int MinPlayCount { get; }

        /// <summary>
        /// </summary>
        private int MaxPlayCount { get; }

        /// <summary>
        /// </summary>
        private long UploadStartDate { get; }

        /// <summary>
        /// </summary>
        private long UploadEndDate { get; }

        /// <summary>
        /// </summary>
        private long LastUpdatedStartDate { get; }

        /// <summary>
        /// </summary>
        private long LastUpdatedEndDate { get; }

        /// <summary>
        /// </summary>
        private int Page { get; }

        /// <summary>
        /// </summary>
        /// <param name="query"></param>
        /// <param name="mode"></param>
        /// <param name="status"></param>
        /// <param name="minDiff"></param>
        /// <param name="maxDiff"></param>
        /// <param name="minBpm"></param>
        /// <param name="maxBpm"></param>
        /// <param name="minLength"></param>
        /// <param name="maxLength"></param>
        /// <param name="minln"></param>
        /// <param name="maxln"></param>
        /// <param name="minPlayCount"></param>
        /// <param name="maxPlayCount"></param>
        /// <param name="startUploadDate"></param>
        /// <param name="endUploadDate"></param>
        /// <param name="startUpdateDate"></param>
        /// <param name="endUpdateDate"></param>
        /// <param name="page"></param>
        public APIRequestMapsetSearch(string query, DownloadFilterMode mode, DownloadFilterRankedStatus status, float minDiff,
            float maxDiff, float minBpm, float maxBpm, int minLength, int maxLength, int minln, int maxln, int minPlayCount,
            int maxPlayCount, string startUploadDate, string endUploadDate, string startUpdateDate, string endUpdateDate, int page)
        {
            Query = query;
            Mode = mode;
            Status = status;
            MinDiffifculty = minDiff;
            MaxDifficulty = maxDiff;
            MinBpm = minBpm;
            MaxBpm = maxBpm;
            MinLength = minLength;
            MaxLength = maxLength;
            MinLongNotePercent = minln;
            MaxLongNotePercent = maxln;
            MinPlayCount = minPlayCount;
            MaxPlayCount = maxPlayCount;
            Page = page;

            // Upload Date
            if (string.IsNullOrEmpty(startUploadDate))
                startUploadDate = "01-01-0000";

            if (string.IsNullOrEmpty(endUploadDate))
                endUploadDate = "12-31-9999";

            DateTime.TryParse(startUploadDate, out var startDate);
            DateTime.TryParse(endUploadDate, out var endDate);

            UploadStartDate = (long) DateTimeToUnixTimestamp(startDate);
            UploadEndDate = (long) DateTimeToUnixTimestamp(endDate);

            // Update Date
            if (string.IsNullOrEmpty(startUpdateDate))
                startUpdateDate = "01-01-0000";

            if (string.IsNullOrEmpty(endUpdateDate))
                endUpdateDate = "12-31-9999";

            DateTime.TryParse(startUpdateDate, out var startLastUpdateDate);
            DateTime.TryParse(endUpdateDate, out var endLastUpdateDate);

            LastUpdatedStartDate = (long) DateTimeToUnixTimestamp(startLastUpdateDate);
            LastUpdatedEndDate = (long) DateTimeToUnixTimestamp(endLastUpdateDate);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override APIResponseMapsetSearch ExecuteRequest()
        {
            try
            {
                var endpoint = "http://localhost:8082/v1/";
                //var endpoint = "https://api.quavergame.com/v1/";

                var request = new RestRequest($"{endpoint}mapsets/maps/search", Method.GET);
                var client = new RestClient(endpoint) { UserAgent = "Quaver" };

                request.AddQueryParameter("search", Query);
                SetModeQueryParams(request);
                SetStatusQueryParams(request);
                request.AddQueryParameter("mindiff", MinDiffifculty.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("maxdiff", MaxDifficulty.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("minbpm", MinBpm.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("maxbpm", MaxBpm.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("minlength", MinLength.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("maxlength", MaxLength.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("minlns", MinLongNotePercent.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("maxlns", MaxLongNotePercent.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("mindate", UploadStartDate.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("maxdate", UploadEndDate.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("mindatelastupdated",LastUpdatedStartDate.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("maxdatelastupdated",LastUpdatedEndDate.ToString(CultureInfo.InvariantCulture));

                request.AddQueryParameter("page", Page.ToString());

                var response = client.Execute(request);
                var json = JObject.Parse(response.Content);

                return JsonConvert.DeserializeObject<APIResponseMapsetSearch>(json.ToString());
            }
            catch (Exception e)
            {
                Logger.Error(e,LogType.Runtime);

                return new APIResponseMapsetSearch
                {
                    Status = -1,
                    Mapsets = new List<DownloadableMapset>()
                };
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        private void SetModeQueryParams(RestRequest request)
        {
            // Game Mode Query Param
            if (Mode == DownloadFilterMode.All)
            {
                foreach (DownloadFilterMode mode in Enum.GetValues(typeof(DownloadFilterMode)))
                {
                    if (mode == DownloadFilterMode.All)
                        continue;

                    request.AddQueryParameter("mode", ((int) mode).ToString());
                }
            }
            else
                request.AddQueryParameter("mode", ((int) Mode).ToString());
        }

        /// <summary>
        /// </summary>
        private void SetStatusQueryParams(RestRequest request)
        {
            // Ranked Status Query Param
            if (Status == DownloadFilterRankedStatus.All)
            {
                foreach (DownloadFilterRankedStatus status in Enum.GetValues(typeof(DownloadFilterRankedStatus)))
                {
                    if (status == DownloadFilterRankedStatus.All)
                        continue;

                    request.AddQueryParameter("status", ((int) status).ToString());
                }
            }
            else
            {
                request.AddQueryParameter("status", ((int) Status).ToString());
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var unixTimeStampInTicks = (dateTime.ToUniversalTime() - unixStart).Ticks;

            return (double) unixTimeStampInTicks / TimeSpan.TicksPerSecond * 1000;
        }
    }
}