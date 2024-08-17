using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;

namespace Quaver.Shared.Database.Maps;

public static class MapsetInfoRetriever
{
    /// <summary>
    ///     Info retrieval requests yet to be processed
    /// </summary>
    private static readonly ConcurrentQueue<MapsetInfoRequest> Queue = new();

    /// <summary>
    ///     <see cref="CancellationTokenSource"/> that can be used to cancel all running tasks
    /// </summary>
    private static CancellationTokenSource TokenSource { get; set; }

    /// <summary>
    ///     Number of concurrent workers to process the info request
    /// </summary>
    private const int WorkerCount = 5;

    /// <summary>
    ///     Invoked when a map's info has been successfully retrieved and updated
    /// </summary>
    public static event EventHandler<Map> MapsetInfoRetrieved;

    /// <summary>
    ///     Enqueues a map to be updated
    /// </summary>
    /// <param name="mapId"></param>
    /// <param name="mapsetId"></param>
    public static void Enqueue(Map map)
    {
        Queue.Enqueue(new MapsetInfoRequest(map));
        NotificationManager.Show(NotificationLevel.Info, $"Retrieving status for mapset {map.MapId}");

        if (TokenSource == null)
            StartWorkers();
    }

    public static void StartWorkers()
    {
        StopWorkers();
        for (var i = 0; i < WorkerCount; i++)
        {
            Task.Run(() => Worker(TokenSource.Token));
        }
    }

    public static void StopWorkers()
    {
        TokenSource?.Cancel();
        TokenSource = new CancellationTokenSource();
    }

    private static Map FindMap(int mapId, int mapsetId)
    {
        var map = MapManager.FindMapFromOnlineId(mapId);
        map ??= MapDatabaseCache.FindSet(mapsetId);
        return map;
    }

    private static async Task Worker(CancellationToken cancellationToken)
    {
        while (true)
        {
            await Task.Delay(100, cancellationToken);

            if (!Queue.TryDequeue(out var request))
                continue;

            request.MapsResponse ??= OnlineManager.Client?.RetrieveMapInfo(request.Map.Id);
            if (request.MapsResponse == null)
            {
                Requeue(request);
                continue;
            }

            request.Map.RankedStatus = request.MapsResponse.Map.RankedStatus;
            request.Map.DateLastUpdated = request.MapsResponse.Map.DateLastUpdated;
            request.Map.OnlineOffset = request.MapsResponse.Map.OnlineOffset;
            MapDatabaseCache.UpdateMap(request.Map);
            NotificationManager.Show(NotificationLevel.Info, $"Info for mapset {request} has been updated");
            MapsetInfoRetrieved?.Invoke(null, request.Map);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static void Requeue(MapsetInfoRequest request)
    {
        request.RetrieveAttempts++;
        if (request.RetrieveAttempts >= MapsetInfoRequest.MaxRetrieveAttempts)
            return;
        Queue.Enqueue(request);
    }

    private struct MapsetInfoRequest
    {
        public const int MaxRetrieveAttempts = 5;
        public readonly Map Map;
        public int RetrieveAttempts;
        public MapsResponse MapsResponse;

        public MapsetInfoRequest(Map map) : this()
        {
            Map = map;
        }
    }
}