using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit.UI.Playfield
{
    internal sealed class EditorPlayfieldChunkCoordinator<TSlice, TChunk>
        where TSlice : Drawable
        where TChunk : class, IDisposable
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
        private readonly int _prefetchChunkCount;
        private readonly Func<int, Func<bool>, CancellationToken, TChunk?> _generateChunk;
        private readonly Func<TChunk, TSlice> _createSlice;
        private readonly Action _disposeDecoder;

        private readonly List<int> _requestedChunks = new();
        private readonly List<int> _visibleChunks = new();
        private readonly List<int> _requestedChunkBuffer = new();
        private bool _isDestroyed;

        public bool IsActive { get; set; }

        public EditorPlayfieldChunkCoordinator(EditorPlayfield playfield, double trackLengthMilliseconds,
            double chunkLengthMilliseconds, Func<int, Func<bool>, CancellationToken, TChunk?> generateChunk,
            Func<TChunk, TSlice> createSlice, Action disposeDecoder, int prefetchChunkCount,
            CancellationToken cancellationToken)
        {
            _playfield = playfield;
            _trackLengthMilliseconds = trackLengthMilliseconds;
            _chunkLengthMilliseconds = chunkLengthMilliseconds;
            _prefetchChunkCount = prefetchChunkCount;
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
                UpdateVisibleChunks();
                UpdateRequestedChunks();
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

            while (_completedChunks.TryDequeue(out var completed))
            {
                completed.Chunk.Dispose();
                _completedChunkSlots.Release();
            }
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
                                chunk?.Dispose();
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

                                    chunk.Dispose();
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
                completed.Chunk.Dispose();
                _completedChunkSlots.Release();
                SignalWorker();
            }
        }

        private void UpdateVisibleChunks()
        {
            _visibleChunks.Clear();

            if (_trackLengthMilliseconds <= 0 || _chunkLengthMilliseconds <= 0)
                return;

            var trackSpeed = Math.Max(_playfield.TrackSpeed, float.Epsilon);
            var currentTime = Math.Clamp(Audio.AudioEngine.Track.Time, 0, _trackLengthMilliseconds);
            var pastWindow = Math.Max(0, _playfield.Height - _playfield.HitPositionY) / trackSpeed;
            var futureWindow = Math.Max(0, _playfield.HitPositionY) / trackSpeed;
            var visibleStart = Math.Max(0, currentTime - pastWindow);
            var visibleEnd = Math.Min(_trackLengthMilliseconds, currentTime + futureWindow);
            var chunkCount = (int)Math.Ceiling(_trackLengthMilliseconds / _chunkLengthMilliseconds);
            var first = Math.Clamp((int)(visibleStart / _chunkLengthMilliseconds), 0, chunkCount - 1);
            var last = Math.Clamp((int)(visibleEnd / _chunkLengthMilliseconds), first, chunkCount - 1);
            for (var index = first; index <= last; index++)
                _visibleChunks.Add(index);
        }

        private void UpdateRequestedChunks()
        {
            _requestedChunkBuffer.Clear();

            if (_visibleChunks.Count == 0)
            {
                SetRequestedChunks(_requestedChunkBuffer);
                return;
            }

            var chunkCount = (int)Math.Ceiling(_trackLengthMilliseconds / _chunkLengthMilliseconds);
            var currentIndex = Math.Clamp((int)(Audio.AudioEngine.Track.Time / _chunkLengthMilliseconds),
                0, chunkCount - 1);
            var firstVisible = _visibleChunks[0];
            var lastVisible = _visibleChunks[^1];
            currentIndex = Math.Clamp(currentIndex, firstVisible, lastVisible);
            _requestedChunkBuffer.Add(currentIndex);

            for (var distance = 1; _requestedChunkBuffer.Count < _visibleChunks.Count; distance++)
            {
                var after = currentIndex + distance;

                if (after <= lastVisible)
                    _requestedChunkBuffer.Add(after);

                var before = currentIndex - distance;

                if (before >= firstVisible)
                    _requestedChunkBuffer.Add(before);
            }

            for (var distance = 1; distance <= _prefetchChunkCount; distance++)
            {
                var after = lastVisible + distance;

                if (after < chunkCount)
                    _requestedChunkBuffer.Add(after);
            }

            for (var distance = 1; distance <= _prefetchChunkCount; distance++)
            {
                var before = firstVisible - distance;

                if (before >= 0)
                    _requestedChunkBuffer.Add(before);
            }

            SetRequestedChunks(_requestedChunkBuffer);
        }

        private void SetRequestedChunks(IReadOnlyList<int> requestedChunks)
        {
            lock (_stateLock)
            {
                if (RequestedChunksEqual(requestedChunks))
                    return;

                _requestedChunks.Clear();

                for (var index = 0; index < requestedChunks.Count; index++)
                    _requestedChunks.Add(requestedChunks[index]);
            }

            SignalWorker();
        }

        private bool RequestedChunksEqual(IReadOnlyList<int> requestedChunks)
        {
            if (_requestedChunks.Count != requestedChunks.Count)
                return false;

            for (var index = 0; index < requestedChunks.Count; index++)
            {
                if (_requestedChunks[index] != requestedChunks[index])
                    return false;
            }

            return true;
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
