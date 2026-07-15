using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit.UI.Playfield
{
    internal sealed class EditorPlayfieldChunkCoordinator<TSlice, TChunk>
        where TSlice : Drawable
        where TChunk : class
    {
        private const int CompletedChunkLimit = 2;

        private readonly object _stateLock = new();
        private readonly Dictionary<int, ChunkState> _chunkStates = new();
        private readonly Dictionary<int, TSlice> _cachedSlices = new();
        private readonly ConcurrentQueue<CompletedChunk> _completedChunks = new();
        private readonly SemaphoreSlim _completedChunkSlots = new(CompletedChunkLimit, CompletedChunkLimit);
        private readonly SemaphoreSlim _requestSignal = new(0, 1);
        private readonly CancellationTokenSource _cancellationSource;
        private readonly EditorPlayfield _playfield;
        private readonly double _trackLengthMilliseconds;
        private readonly double _chunkLengthMilliseconds;
        private readonly Func<int, Func<bool>, CancellationToken, TChunk?> _generateChunk;
        private readonly Func<TChunk, TSlice> _createSlice;
        private readonly Action _disposeDecoder;

        private List<int> _requestedChunks = new();
        private List<int> _visibleChunks = new();
        private bool _isDestroyed;

        public bool IsActive { get; set; }

        public EditorPlayfieldChunkCoordinator(EditorPlayfield playfield, double trackLengthMilliseconds,
            double chunkLengthMilliseconds, Func<int, Func<bool>, CancellationToken, TChunk?> generateChunk,
            Func<TChunk, TSlice> createSlice, Action disposeDecoder, CancellationToken cancellationToken)
        {
            _playfield = playfield;
            _trackLengthMilliseconds = trackLengthMilliseconds;
            _chunkLengthMilliseconds = chunkLengthMilliseconds;
            _generateChunk = generateChunk;
            _createSlice = createSlice;
            _disposeDecoder = disposeDecoder;
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Task.Run(RunWorkerAsync);
        }

        public void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                _visibleChunks = GetVisibleChunks();
                SetRequestedChunks(GetRequestedChunks(_visibleChunks));
            }
            else
            {
                _visibleChunks.Clear();
                SetRequestedChunks(Array.Empty<int>());
            }

            UploadCompletedChunk();

            foreach (var index in _visibleChunks)
            {
                if (_cachedSlices.TryGetValue(index, out var slice))
                    slice.Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime)
        {
            if (!IsActive)
                return;

            foreach (var index in _visibleChunks)
            {
                if (_cachedSlices.TryGetValue(index, out var slice))
                    slice.Draw(gameTime);
            }
        }

        public void Destroy()
        {
            if (_isDestroyed)
                return;

            _isDestroyed = true;
            IsActive = false;

            lock (_stateLock)
            {
                _requestedChunks.Clear();
                _chunkStates.Clear();
            }

            _cancellationSource.Cancel();
            SignalWorker();

            foreach (var slice in _cachedSlices.Values)
                slice.Destroy();

            _cachedSlices.Clear();

            while (_completedChunks.TryDequeue(out _))
                _completedChunkSlots.Release();
        }

        private async Task RunWorkerAsync()
        {
            var token = _cancellationSource.Token;

            try
            {
                while (true)
                {
                    await _requestSignal.WaitAsync(token);

                    while (TryBeginNextChunk(out var index))
                    {
                        await _completedChunkSlots.WaitAsync(token);

                        if (!IsRequested(index))
                        {
                            MarkMissing(index);
                            _completedChunkSlots.Release();
                            continue;
                        }

                        try
                        {
                            var chunk = _generateChunk(index, () => IsRequested(index), token);

                            if (chunk == null || !IsRequested(index))
                            {
                                MarkMissing(index);
                                _completedChunkSlots.Release();
                                continue;
                            }

                            lock (_stateLock)
                            {
                                if (_isDestroyed || !_requestedChunks.Contains(index))
                                {
                                    if (!_isDestroyed)
                                        _chunkStates[index] = ChunkState.Missing;

                                    _completedChunkSlots.Release();
                                    continue;
                                }

                                _chunkStates[index] = ChunkState.CpuReady;
                                _completedChunks.Enqueue(new CompletedChunk(index, chunk));
                            }
                        }
                        catch (OperationCanceledException) when (!token.IsCancellationRequested)
                        {
                            MarkMissing(index);
                            _completedChunkSlots.Release();
                        }
                        catch (OperationCanceledException)
                        {
                            _completedChunkSlots.Release();
                            throw;
                        }
                        catch (Exception e)
                        {
                            lock (_stateLock)
                                _chunkStates[index] = ChunkState.Failed;

                            _completedChunkSlots.Release();
                            Logger.Error(e, LogType.Runtime);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when the visualization is reloaded or destroyed.
            }
            finally
            {
                try
                {
                    _disposeDecoder();
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }
        }

        private void UploadCompletedChunk()
        {
            if (!_completedChunks.TryDequeue(out var completed))
                return;

            try
            {
                if (_isDestroyed || !IsRequested(completed.Index))
                {
                    MarkMissing(completed.Index);
                    return;
                }

                var slice = _createSlice(completed.Chunk);
                _cachedSlices[completed.Index] = slice;

                lock (_stateLock)
                    _chunkStates[completed.Index] = ChunkState.Loaded;
            }
            catch (Exception e)
            {
                lock (_stateLock)
                    _chunkStates[completed.Index] = ChunkState.Failed;

                Logger.Error(e, LogType.Runtime);
            }
            finally
            {
                _completedChunkSlots.Release();
                SignalWorker();
            }
        }

        private List<int> GetVisibleChunks()
        {
            if (_trackLengthMilliseconds <= 0 || _chunkLengthMilliseconds <= 0)
                return new List<int>();

            var trackSpeed = Math.Max(_playfield.TrackSpeed, float.Epsilon);
            var currentTime = Math.Clamp(Audio.AudioEngine.Track.Time, 0, _trackLengthMilliseconds);
            var pastWindow = Math.Max(0, _playfield.Height - _playfield.HitPositionY) / trackSpeed;
            var futureWindow = Math.Max(0, _playfield.HitPositionY) / trackSpeed;
            var visibleStart = Math.Max(0, currentTime - pastWindow);
            var visibleEnd = Math.Min(_trackLengthMilliseconds, currentTime + futureWindow);
            var chunkCount = (int)Math.Ceiling(_trackLengthMilliseconds / _chunkLengthMilliseconds);
            var first = Math.Clamp((int)(visibleStart / _chunkLengthMilliseconds), 0, chunkCount - 1);
            var last = Math.Clamp((int)(visibleEnd / _chunkLengthMilliseconds), first, chunkCount - 1);
            var result = new List<int>(last - first + 1);

            for (var index = first; index <= last; index++)
                result.Add(index);

            return result;
        }

        private List<int> GetRequestedChunks(IReadOnlyList<int> visibleChunks)
        {
            if (visibleChunks.Count == 0)
                return new List<int>();

            var chunkCount = (int)Math.Ceiling(_trackLengthMilliseconds / _chunkLengthMilliseconds);
            var currentIndex = Math.Clamp((int)(Audio.AudioEngine.Track.Time / _chunkLengthMilliseconds),
                0, chunkCount - 1);
            var requested = visibleChunks.OrderBy(index => Math.Abs(index - currentIndex)).ToList();
            var before = visibleChunks[0] - 1;
            var after = visibleChunks[^1] + 1;

            if (before >= 0)
                requested.Add(before);

            if (after < chunkCount)
                requested.Add(after);

            return requested;
        }

        private void SetRequestedChunks(IEnumerable<int> requestedChunks)
        {
            var requested = requestedChunks.ToList();

            lock (_stateLock)
            {
                if (_requestedChunks.SequenceEqual(requested))
                    return;

                _requestedChunks = requested;
            }

            SignalWorker();
        }

        private bool TryBeginNextChunk(out int index)
        {
            lock (_stateLock)
            {
                foreach (var requested in _requestedChunks)
                {
                    if (_chunkStates.TryGetValue(requested, out var state) && state != ChunkState.Missing)
                        continue;

                    _chunkStates[requested] = ChunkState.Generating;
                    index = requested;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        private bool IsRequested(int index)
        {
            lock (_stateLock)
                return !_isDestroyed && _requestedChunks.Contains(index);
        }

        private void MarkMissing(int index)
        {
            lock (_stateLock)
            {
                if (_chunkStates.TryGetValue(index, out var state) && state != ChunkState.Loaded)
                    _chunkStates[index] = ChunkState.Missing;
            }
        }

        private void SignalWorker()
        {
            if (_requestSignal.CurrentCount == 0)
                _requestSignal.Release();
        }

        private enum ChunkState
        {
            Missing,
            Generating,
            CpuReady,
            Loaded,
            Failed
        }

        private sealed record CompletedChunk(int Index, TChunk Chunk);
    }
}
