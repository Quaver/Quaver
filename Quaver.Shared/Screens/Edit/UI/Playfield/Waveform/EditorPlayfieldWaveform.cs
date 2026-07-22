using System;
using System.Buffers;
using System.Threading;
using ManagedBass;
using ManagedBass.Fx;
using Microsoft.Xna.Framework;
using Quaver.Shared.Config;
using Wobble.Audio.Tracks;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Waveform
{
    public class EditorPlayfieldWaveform : Container
    {
        private const double ChunkLength = 500;
        private const int PrefetchChunkCount = 3;
        private const double DecoderPreRollMilliseconds = 100;

        private readonly EditorPlayfieldWaveformFilter _filter;
        private readonly int _leftChannel;
        private readonly int _rightChannel;
        private readonly int _channelCount;
        private readonly int _textureWidth;

        private int _stream;

        private readonly EditorPlayfield _playfield;
        private readonly EditorPlayfieldChunkCoordinator<EditorPlayfieldWaveformSlice,
            EditorPlayfieldWaveformChunk> _chunks;

        public bool IsActive
        {
            get => _chunks.IsActive;
            set => _chunks.IsActive = value;
        }

        public EditorPlayfieldWaveform(EditorPlayfield playfield, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            _playfield = playfield;
            _filter = ConfigManager.EditorAudioFilter.Value;
            _textureWidth = Math.Max(1, (int)playfield.Width);

            var direction = ConfigManager.EditorAudioDirection.Value;
            _leftChannel = direction == EditorPlayfieldWaveformAudioDirection.Right ? 1 : 0;
            _rightChannel = direction == EditorPlayfieldWaveformAudioDirection.Left ? 0 : 1;

            _stream = Bass.CreateStream(((AudioTrack)Audio.AudioEngine.Track).OriginalFilePath, 0, 0,
                BassFlags.Decode | BassFlags.Float | BassFlags.Prescan);

            if (_stream == 0)
                throw new InvalidOperationException($"Could not create waveform decode stream: {Bass.LastError}");

            var channelInfo = Bass.ChannelGetInfo(_stream);
            _channelCount = Math.Max(1, channelInfo.Channels);
            TrackLengthMilliseconds = Bass.ChannelBytes2Seconds(_stream, Bass.ChannelGetLength(_stream)) * 1000;

            if (_filter != EditorPlayfieldWaveformFilter.None)
            {
                var effect = Bass.ChannelSetFX(_stream, EffectType.BQF, 1);
                var parameters = new BQFParameters
                {
                    fCenter = 200,
                    lFilter = _filter == EditorPlayfieldWaveformFilter.LowPass ? BQFType.LowPass : BQFType.HighPass
                };

                Bass.FXSetParameters(effect, parameters);
            }

            _chunks = new EditorPlayfieldChunkCoordinator<EditorPlayfieldWaveformSlice,
                EditorPlayfieldWaveformChunk>(playfield, TrackLengthMilliseconds, ChunkLength, GenerateChunk,
                CreateSlice, DisposeDecoder, PrefetchChunkCount, token);
        }

        public override void Update(GameTime gameTime)
        {
            _chunks.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime) => _chunks.Draw(gameTime);

        public override void Destroy()
        {
            if (_chunks == null)
                DisposeDecoder();
            else
                _chunks.Destroy();

            base.Destroy();
        }

        private double TrackLengthMilliseconds { get; }

        private EditorPlayfieldWaveformChunk? GenerateChunk(int index, Func<bool> isStillRequested,
            CancellationToken cancellationToken)
        {
            var startTime = index * ChunkLength;
            var endTime = Math.Min(TrackLengthMilliseconds, startTime + ChunkLength);
            var length = endTime - startTime;

            if (length <= 0)
                return null;

            // Compressed formats such as MP3 can output a short run of silence after a random seek while
            // their decoder state is rebuilt. Decode a small overlap before every chunk and discard it.
            // This also supplies the history required by the optional BQF filter.
            var decodeStartTime = Math.Max(0, startTime - DecoderPreRollMilliseconds);
            var decodeStartByte = Bass.ChannelSeconds2Bytes(_stream, decodeStartTime / 1000);
            var decodeEndByte = Bass.ChannelSeconds2Bytes(_stream, endTime / 1000);
            var requestedBytes = (int)Math.Min(int.MaxValue, Math.Max(0, decodeEndByte - decodeStartByte));
            var requestedSampleCount = (requestedBytes + sizeof(float) - 1) / sizeof(float);
            var samples = ArrayPool<float>.Shared.Rent(requestedSampleCount);

            try
            {
                if (!Bass.ChannelSetPosition(_stream, decodeStartByte, PositionFlags.Bytes))
                    throw new InvalidOperationException($"Could not seek waveform chunk {index}: {Bass.LastError}");

                var bytesRead = Bass.ChannelGetData(_stream, samples, requestedBytes);

                if (bytesRead < 0)
                    throw new InvalidOperationException($"Could not decode waveform chunk {index}: {Bass.LastError}");

                var sampleCount = bytesRead / sizeof(float);
                var textureHeight = Math.Max(1, (int)Math.Ceiling(length));
                var pixelCount = _textureWidth * textureHeight;
                var pixels = ArrayPool<Color>.Shared.Rent(pixelCount);
                var pixelOwnershipTransferred = false;

                try
                {
                    Array.Clear(pixels, 0, pixelCount);
                    var leftChannel = Math.Min(_leftChannel, _channelCount - 1);
                    var rightChannel = Math.Min(_rightChannel, _channelCount - 1);

                    for (var y = 0; y < textureHeight; y++)
                    {
                        if ((y & 31) == 0)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (!isStillRequested())
                                return null;
                        }

                        var time = Math.Min(endTime, startTime + y);
                        var sampleByte = Bass.ChannelSeconds2Bytes(_stream, time / 1000);
                        var sampleIndex = (int)((sampleByte - decodeStartByte) / sizeof(float));

                        if (sampleIndex < 0 || sampleIndex + leftChannel >= sampleCount ||
                            sampleIndex + rightChannel >= sampleCount)
                            continue;

                        var leftPoint = (int)Math.Clamp(_textureWidth / 2f -
                            Math.Abs(samples[sampleIndex + leftChannel] / 1.5f) * 254,
                            0, _textureWidth / 2f);
                        var rightPoint = (int)Math.Clamp(_textureWidth / 2f +
                            Math.Abs(samples[sampleIndex + rightChannel] / 1.5f) * 254,
                            _textureWidth / 2f, _textureWidth);

                        for (var x = leftPoint; x < rightPoint; x++)
                            pixels[(textureHeight - y - 1) * _textureWidth + x] = Color.White;
                    }

                    pixelOwnershipTransferred = true;
                    return new EditorPlayfieldWaveformChunk(pixels, _textureWidth, textureHeight, startTime, length);
                }
                finally
                {
                    if (!pixelOwnershipTransferred)
                        ArrayPool<Color>.Shared.Return(pixels);
                }
            }
            finally
            {
                ArrayPool<float>.Shared.Return(samples);
            }
        }

        private EditorPlayfieldWaveformSlice CreateSlice(EditorPlayfieldWaveformChunk chunk)
            => new(_playfield, chunk.Pixels, chunk.Width, chunk.Height, chunk.StartTime, chunk.Length);

        private void DisposeDecoder()
        {
            if (_stream == 0)
                return;

            Bass.StreamFree(_stream);
            _stream = 0;
        }
    }

    internal sealed class EditorPlayfieldWaveformChunk : IDisposable
    {
        public Color[] Pixels { get; }
        public int Width { get; }
        public int Height { get; }
        public double StartTime { get; }
        public double Length { get; }

        private bool _isDisposed;

        public EditorPlayfieldWaveformChunk(Color[] pixels, int width, int height, double startTime, double length)
        {
            Pixels = pixels;
            Width = width;
            Height = height;
            StartTime = startTime;
            Length = length;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            ArrayPool<Color>.Shared.Return(Pixels);
        }
    }
}
