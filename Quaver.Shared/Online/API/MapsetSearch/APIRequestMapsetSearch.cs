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
        private int MinCombo { get; }

        /// <summary>
        /// </summary>
        private int MaxCombo { get; }

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
        /// <param name="minCombo"></param>
        /// <param name="maxCombo"></param>
        /// <param name="page"></param>
        public APIRequestMapsetSearch(string query, DownloadFilterMode mode, DownloadFilterRankedStatus status, float minDiff,
            float maxDiff, float minBpm, float maxBpm, int minLength, int maxLength, int minln, int maxln, int minPlayCount,
            int maxPlayCount, string startUploadDate, string endUploadDate, string startUpdateDate, string endUpdateDate,
            int minCombo, int maxCombo, int page)
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
            MinCombo = minCombo;
            MaxCombo = maxCombo;
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
                //var endpoint = "http://localhost:8082/v1/";
                var endpoint = "https://api.quavergame.com/v2/";

                var request = new RestRequest($"{endpoint}mapset/search", Method.GET);
                var client = new RestClient(endpoint) { UserAgent = "Quaver" };

                request.AddQueryParameter("search", Query);
                SetModeQueryParams(request);
                SetStatusQueryParams(request);
                request.AddQueryParameter("min_difficulty_rating", MinDiffifculty.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("max_difficulty_rating", MaxDifficulty.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("min_bpm", MinBpm.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("max_bpm", MaxBpm.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("min_length", MinLength.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("max_length", MaxLength.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("min_long_note_percent", MinLongNotePercent.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("max_long_note_percent", MaxLongNotePercent.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("min_date_submitted", UploadStartDate.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("max_date_submitted", UploadEndDate.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("min_last_updated",LastUpdatedStartDate.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("max_last_updated",LastUpdatedEndDate.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("min_combo", MinCombo.ToString(CultureInfo.InvariantCulture));
                request.AddQueryParameter("max_combo", MaxCombo.ToString(CultureInfo.InvariantCulture));
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

                    request.AddQueryParameter("ranked_status", ((int) status).ToString());
                }
            }
            else
            {
                request.AddQueryParameter("ranked_status", ((int) Status).ToString());
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