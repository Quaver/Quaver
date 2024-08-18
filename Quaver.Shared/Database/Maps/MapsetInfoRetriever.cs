using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;

using Wobble.Logging;

namespace Quaver.Shared.Database.Maps;

public static class MapsetInfoRetriever
{
    /// <summary>
    ///     Info retrieval requests yet to be processed
    /// </summary>
    private static readonly ConcurrentQueue<MapsetInfoRequest> RequestQueue = new();

    private static readonly ConcurrentQueue<MapsetInfoResponse> ResponseQueue = new();

    /// <summary>
    ///     <see cref="CancellationTokenSource"/> that can be used to cancel all running tasks
    /// </summary>
    private static CancellationTokenSource TokenSource { get; set; }

    /// <summary>
    ///     Used to cancel the worker that updates map info and invokes <see cref="MapsetInfoRetrieved"/>
    /// </summary>
    private static CancellationTokenSource InfoUpdateTokenSource { get; set; } = new();

    /// <summary>
    ///     Number of concurrent workers to process the info request
    /// </summary>
    private const int WorkerCount = 5;

    /// <summary>
    ///     Invoked when a map's info has been successfully retrieved and updated
    /// </summary>
    public static event EventHandler MapsetInfoRetrieved;

    private static bool s_infoUpdateEnabled = false;

    /// <summary>
    ///     Whether the responses are processed and updated at the moment.
    ///     Disabled when cache has not been updated to <see cref="MapManager.Mapsets"/>
    /// </summary>
    public static bool InfoUpdateEnabled
    {
        get => s_infoUpdateEnabled;
        set
        {
            s_infoUpdateEnabled = value;
            InfoUpdateTokenSource.Cancel();
            InfoUpdateTokenSource = new CancellationTokenSource();
            if (value)
            {
                Task.Run(() => ApplyMapInfoUpdates(InfoUpdateTokenSource.Token));
            }
        }
    }

    /// <summary>
    ///     Enqueues a map to be updated
    /// </summary>
    /// <param name="mapId"></param>
    /// <param name="mapsetId"></param>
    public static void Enqueue(Map map)
    {
        RequestQueue.Enqueue(new MapsetInfoRequest(map));
        NotificationManager.Show(NotificationLevel.Info, $"Retrieving status for mapset {map.MapId}");

        if (TokenSource == null)
            StartWorkers();
    }

    public static void StartWorkers()
    {
        StopWorkers();
        TokenSource = new CancellationTokenSource();
        for (var i = 0; i < WorkerCount; i++)
        {
            Task.Run(() => Worker(TokenSource.Token));
        }
    }

    public static void StopWorkers()
    {
        TokenSource?.Cancel();
        TokenSource = null;
    }

    private static async Task Worker(CancellationToken cancellationToken)
    {
        while (true)
        {
            await Task.Delay(100, cancellationToken);

            if (!RequestQueue.TryDequeue(out var request))
                continue;

            request.MapsResponse ??= OnlineManager.Client?.RetrieveMapInfo(request.Map.Id);
            if (request.MapsResponse == null)
            {
                Requeue(request);
                continue;
            }

            ResponseQueue.Enqueue(new MapsetInfoResponse(request.Map, request.MapsResponse));
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static async Task ApplyMapInfoUpdates(CancellationToken cancellationToken)
    {
        while (true)
        {
            await Task.Delay(1000, cancellationToken);
            while (ResponseQueue.TryDequeue(out var response))
            {
                if (MapManager.FindMapFromOnlineId(response.Map.MapId) is { } map)
                    response.Map = map;

                response.Map.RankedStatus = response.MapsResponse.Map.RankedStatus;
                response.Map.DateLastUpdated = response.MapsResponse.Map.DateLastUpdated;
                response.Map.OnlineOffset = response.MapsResponse.Map.OnlineOffset;
                MapDatabaseCache.UpdateMap(response.Map);
                Logger.Important(
                    $"Info for map {response.Map.MapId} has been updated", LogType.Runtime);
            }

            MapsetInfoRetrieved?.Invoke(null, EventArgs.Empty);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static void Requeue(MapsetInfoRequest request)
    {
        request.RetrieveAttempts++;
        if (request.RetrieveAttempts >= MapsetInfoRequest.MaxRetrieveAttempts)
            return;
        RequestQueue.Enqueue(request);
    }

    private struct MapsetInfoRequest
    {
        public const int MaxRetrieveAttempts = 5;
        public Map Map;
        public int RetrieveAttempts;
        public MapsResponse MapsResponse;

        public MapsetInfoRequest(Map map) : this()
        {
            Map = map;
        }
    }

    private struct MapsetInfoResponse
    {
        public Map Map;
        public MapsResponse MapsResponse;

        public MapsetInfoResponse(Map map, MapsResponse mapsResponse)
        {
            Map = map;
            MapsResponse = mapsResponse;
        }
    }
}