using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Online;
using Wobble.Logging;

namespace Quaver.Shared.Database.Maps;

public static class MapsetInfoRetriever
{
    /// <summary>
    ///     Info retrieval requests yet to be processed
    /// </summary>
    private static readonly Channel<MapsetInfoRequest> RequestChannel =
        Channel.CreateUnbounded<MapsetInfoRequest>(new UnboundedChannelOptions { SingleWriter = true });

    /// <summary>
    ///     The info retrieved that is yet to be synced to the database
    /// </summary>
    private static readonly Channel<MapsetInfoResponse> ResponseChannel =
        Channel.CreateUnbounded<MapsetInfoResponse>(new UnboundedChannelOptions { SingleReader = true });

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

    private static bool s_infoUpdateEnabled;

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
    public static async Task Enqueue(Map map)
    {
        await RequestChannel.Writer.WriteAsync(new MapsetInfoRequest(map));
        Logger.Debug($"Retrieving status for mapset {map.MapId}", LogType.Runtime);

        if (TokenSource == null)
            StartWorkers();
    }

    /// <summary>
    ///     Enables workers to fetch map online info
    /// </summary>
    private static void StartWorkers()
    {
        StopWorkers();
        TokenSource = new CancellationTokenSource();
        for (var i = 0; i < WorkerCount; i++)
        {
            Task.Run(() => Worker(TokenSource.Token));
        }
    }

    /// <summary>
    ///     Stop all workers. This does not include the sync worker
    /// </summary>
    private static void StopWorkers()
    {
        TokenSource?.Cancel();
        TokenSource = null;
    }

    /// <summary>
    ///     Takes a map in the <see cref="RequestChannel"/> and retrieves its online info.
    ///     If failed for under <see cref="MapsetInfoRequest.MaxRetrieveAttempts"/>,
    ///     the request will be put back on queue for a retry
    /// </summary>
    /// <param name="cancellationToken"></param>
    private static async Task Worker(CancellationToken cancellationToken)
    {
        while (true)
        {
            var request = await RequestChannel.Reader.ReadAsync(cancellationToken);

            request.MapsResponse ??= OnlineManager.Client?.RetrieveMapInfo(request.Map.MapId);
            if (request.MapsResponse == null)
            {
                await Requeue(request);
                continue;
            }

            await ResponseChannel.Writer.WriteAsync(new MapsetInfoResponse(request.Map, request.MapsResponse),
                cancellationToken);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    /// <summary>
    ///     The worker that syncs the map online info fetched to the database and mapset
    /// </summary>
    /// <param name="cancellationToken"></param>
    private static async Task ApplyMapInfoUpdates(CancellationToken cancellationToken)
    {
        while (true)
        {
            var response = await ResponseChannel.Reader.ReadAsync(cancellationToken);

            // We better find 'local' instance, rather than database instance
            // since they aren't the same in terms of reference
            if (MapManager.FindMapFromOnlineId(response.Map.MapId) is { } map)
                response.Map = map;

            response.Map.RankedStatus = response.MapsResponse.Map.RankedStatus;
            response.Map.DateLastUpdated = response.MapsResponse.Map.DateLastUpdated;
            response.Map.OnlineOffset = response.MapsResponse.Map.OnlineOffset;
            MapDatabaseCache.UpdateMap(response.Map);
            Logger.Debug(
                $"Info for map {response.Map.MapId} has been updated", LogType.Runtime);

            MapsetInfoRetrieved?.Invoke(null, EventArgs.Empty);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    /// <summary>
    ///     Checks if the request has been retried for under <see cref="MapsetInfoRequest.MaxRetrieveAttempts"/>.
    ///     If yes, puts the request back to the <see cref="RequestChannel"/> to check
    /// </summary>
    /// <param name="request"></param>
    private static async Task Requeue(MapsetInfoRequest request)
    {
        request.RetrieveAttempts++;
        if (request.RetrieveAttempts >= MapsetInfoRequest.MaxRetrieveAttempts)
            return;
        await RequestChannel.Writer.WriteAsync(request);
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

    private struct MapsetInfoResponse
    {
        public Map Map;
        public readonly MapsResponse MapsResponse;

        public MapsetInfoResponse(Map map, MapsResponse mapsResponse)
        {
            Map = map;
            MapsResponse = mapsResponse;
        }
    }
}