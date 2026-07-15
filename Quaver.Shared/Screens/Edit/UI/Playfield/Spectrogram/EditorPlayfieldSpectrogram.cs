using System;
using System.Buffers;
using System.Threading;
using ManagedBass;
using Microsoft.Xna.Framework;
using Quaver.Shared.Config;
using Wobble.Audio.Tracks;
using Wobble.Graphics;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Spectrogram
{
    public class EditorPlayfieldSpectrogram : Container
    {
        private const double TargetChunkLengthMilliseconds = 500;
        private const int PrefetchChunkCount = 2;
        private const double DecoderPreRollMilliseconds = 100;

        private readonly EditorPlayfield _playfield;
        private readonly EditorPlayfieldChunkCoordinator<EditorPlayfieldSpectrogramSlice,
            EditorPlayfieldSpectrogramChunk> _chunks;
        private readonly int _interleaveCount;
        private readonly int _bytesReadPerFft;
        private readonly int _fftStepsPerChunk;
        private readonly int _totalFftSteps;
        private readonly int _sampleRate;
        private readonly int _decoderPreRollBytes;
        private readonly double _millisecondsPerFft;
        private readonly double _chunkLengthMilliseconds;
        private readonly Func<float, float> _frequencyTransform;
        private readonly int _minimumFrequency;
        private readonly int _maximumFrequency;
        private readonly float _cutoffFactor;
        private readonly float _intensityFactor;

        private int _stream;

        public int FftCount { get; }

        public int FftFlag { get; }

        public int FftResultCount { get; set; }

        public bool IsActive
        {
            get => _chunks.IsActive;
            set => _chunks.IsActive = value;
        }

        public EditorPlayfieldSpectrogram(EditorPlayfield playfield, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            _playfield = playfield;
            FftCount = ConfigManager.EditorSpectrogramFftSize.Value;
            FftResultCount = FftCount;
            FftFlag = (int)GetFftDataFlag(FftCount);
            _interleaveCount = ConfigManager.EditorSpectrogramInterleaveCount.Value;
            _cutoffFactor = ConfigManager.EditorSpectrogramCutoffFactor.Value;
            _intensityFactor = ConfigManager.EditorSpectrogramIntensityFactor.Value;
            _frequencyTransform = ConfigManager.EditorSpectrogramFrequencyScale.Value switch
            {
                EditorPlayfieldSpectrogramFrequencyScale.Mel => Mel,
                EditorPlayfieldSpectrogramFrequencyScale.Erb1 => Erb1,
                EditorPlayfieldSpectrogramFrequencyScale.Erb2 => Erb2,
                EditorPlayfieldSpectrogramFrequencyScale.Linear => Linear,
                _ => throw new ArgumentOutOfRangeException()
            };
            _minimumFrequency = (int)_frequencyTransform(ConfigManager.EditorSpectrogramMinimumFrequency.Value);
            _maximumFrequency = (int)_frequencyTransform(ConfigManager.EditorSpectrogramMaximumFrequency.Value);

            _stream = Bass.CreateStream(((AudioTrack)Audio.AudioEngine.Track).OriginalFilePath, 0, 0,
                BassFlags.Decode | BassFlags.Float);

            if (_stream == 0)
                throw new InvalidOperationException($"Could not create spectrogram decode stream: {Bass.LastError}");

            var channelInfo = Bass.ChannelGetInfo(_stream);
            _sampleRate = channelInfo.Frequency;
            _bytesReadPerFft = sizeof(float) * FftResultCount * Math.Max(1, channelInfo.Channels) * 2;
            _decoderPreRollBytes = (int)Bass.ChannelSeconds2Bytes(_stream,
                DecoderPreRollMilliseconds / 1000);
            _millisecondsPerFft = Bass.ChannelBytes2Seconds(_stream, _bytesReadPerFft) * 1000;
            _fftStepsPerChunk = Math.Max(1,
                (int)Math.Ceiling(TargetChunkLengthMilliseconds / _millisecondsPerFft));
            _chunkLengthMilliseconds = _fftStepsPerChunk * _millisecondsPerFft;

            var trackByteLength = Bass.ChannelGetLength(_stream);
            var trackLengthMilliseconds = Bass.ChannelBytes2Seconds(_stream, trackByteLength) * 1000;
            _totalFftSteps = Math.Max(1, (int)Math.Ceiling((double)trackByteLength / _bytesReadPerFft));

            Logger.Debug($"Precision of spectrogram: {_millisecondsPerFft / _interleaveCount}ms",
                LogType.Runtime);

            _chunks = new EditorPlayfieldChunkCoordinator<EditorPlayfieldSpectrogramSlice,
                EditorPlayfieldSpectrogramChunk>(playfield, trackLengthMilliseconds, _chunkLengthMilliseconds,
                GenerateChunk, CreateSlice, DisposeDecoder, PrefetchChunkCount, token);
        }

        public override void Update(GameTime gameTime)
        {
            _chunks.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime) => _chunks.Draw(gameTime);

        public override void Destroy()
        {
            _chunks.Destroy();
            base.Destroy();
        }

        public static DataFlags GetFftDataFlag(int fftSize)
        {
            return fftSize switch
            {
                256 => DataFlags.FFT512,
                512 => DataFlags.FFT1024,
                1024 => DataFlags.FFT2048,
                2048 => DataFlags.FFT4096,
                4096 => DataFlags.FFT8192,
                8192 => DataFlags.FFT16384,
                16384 => DataFlags.FFT32768,
                _ => throw new InvalidOperationException(
                    $"Expected FFT sample size to be between 256 and 16384 and power of 2, found {fftSize}")
            };
        }

        private EditorPlayfieldSpectrogramChunk? GenerateChunk(int index, Func<bool> isStillRequested,
            CancellationToken cancellationToken)
        {
            var startStep = index * _fftStepsPerChunk;
            var stepCount = Math.Min(_fftStepsPerChunk, _totalFftSteps - startStep);

            if (stepCount <= 0)
                return null;

            var rowCount = stepCount * _interleaveCount;
            var fftData = new float[rowCount][];

            try
            {
                for (var row = 0; row < fftData.Length; row++)
                {
                    fftData[row] = ArrayPool<float>.Shared.Rent(FftResultCount);
                    Array.Clear(fftData[row], 0, FftResultCount);
                }

                for (var interleaveRound = 0; interleaveRound < _interleaveCount; interleaveRound++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!isStillRequested())
                        return null;

                    var position = (long)startStep * _bytesReadPerFft +
                        (long)_bytesReadPerFft * interleaveRound / _interleaveCount;
                    SeekWithPreRoll(position, index);

                    for (var step = 0; step < stepCount; step++)
                    {
                        if ((step & 15) == 0)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (!isStillRequested())
                                return null;
                        }

                        var row = step * _interleaveCount + interleaveRound;
                        var bytesRead = Bass.ChannelGetData(_stream, fftData[row],
                            FftFlag | (int)DataFlags.FFTRemoveDC);

                        if (bytesRead <= 0)
                            break;
                    }
                }

                var pixels = GeneratePixels(fftData, isStillRequested, cancellationToken);

                if (pixels == null)
                    return null;

                var startTime = startStep * _millisecondsPerFft;
                var length = stepCount * _millisecondsPerFft;
                return new EditorPlayfieldSpectrogramChunk(pixels, FftCount, rowCount, startTime, length);
            }
            finally
            {
                foreach (var row in fftData)
                {
                    if (row != null)
                        ArrayPool<float>.Shared.Return(row);
                }
            }
        }

        private Color[]? GeneratePixels(float[][] fftData, Func<bool> isStillRequested,
            CancellationToken cancellationToken)
        {
            var textureHeight = fftData.Length;
            var pixelCount = FftCount * textureHeight;
            var pixels = ArrayPool<Color>.Shared.Rent(pixelCount);
            var ownershipTransferred = false;

            try
            {
                Array.Clear(pixels, 0, pixelCount);

                for (var y = 0; y < textureHeight; y++)
                {
                    if ((y & 15) == 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!isStillRequested())
                            return null;
                    }

                    for (var x = 0; x < FftCount; x++)
                    {
                        var textureX = CalculateTextureX(x);

                        if (textureX == -1)
                            continue;

                        var intensity = GetIntensity(fftData[y][x]);
                        var nextTextureX = CalculateTextureX(x + 1);

                        if (nextTextureX == -1)
                            nextTextureX = FftCount - 1;

                        var color = SpectrogramColormap.GetColor(intensity);
                        var start = DataColorIndex(textureHeight, y, textureX);
                        var end = DataColorIndex(textureHeight, y, nextTextureX);

                        for (var pixel = start; pixel < end && pixel < pixelCount; pixel++)
                            pixels[pixel] = color;
                    }
                }

                ownershipTransferred = true;
                return pixels;
            }
            finally
            {
                if (!ownershipTransferred)
                    ArrayPool<Color>.Shared.Return(pixels);
            }
        }

        private float GetIntensity(float rawIntensity)
        {
            var db = MathF.Abs(rawIntensity) < 1e-4f ? -100 : 20 * MathF.Log10(rawIntensity);
            var intensity = Math.Clamp(1 + db / 100, 0f, 1f);
            intensity = MathF.Max(intensity, _cutoffFactor);
            intensity = (intensity - _cutoffFactor) * (1 - _cutoffFactor);
            intensity *= intensity * _intensityFactor;
            intensity = Sigmoid(Math.Clamp(intensity, 0, 1));
            return intensity;
        }

        private int CalculateTextureX(int x)
        {
            var frequency = (float)x * _sampleRate / FftCount;
            var transformedFrequency = _frequencyTransform(frequency);

            if (transformedFrequency > _maximumFrequency || transformedFrequency < _minimumFrequency)
                return -1;

            var textureX = (int)((transformedFrequency - _minimumFrequency) /
                (_maximumFrequency - _minimumFrequency) * FftCount);
            return Math.Clamp(textureX, 0, FftCount - 1);
        }

        private int DataColorIndex(int textureHeight, int y, int x)
            => (textureHeight - y - 1) * FftCount + x;

        private EditorPlayfieldSpectrogramSlice CreateSlice(EditorPlayfieldSpectrogramChunk chunk)
            => new(_playfield, chunk.Pixels, chunk.Width, chunk.Height, chunk.StartTime, chunk.Length);

        private void SeekWithPreRoll(long position, int chunkIndex)
        {
            var decodeStart = Math.Max(0, position - _decoderPreRollBytes);

            if (!Bass.ChannelSetPosition(_stream, decodeStart, PositionFlags.Bytes))
                throw new InvalidOperationException(
                    $"Could not seek spectrogram chunk {chunkIndex}: {Bass.LastError}");

            var bytesToDiscard = (int)(position - decodeStart);

            if (bytesToDiscard == 0)
                return;

            var preRoll = ArrayPool<float>.Shared.Rent(
                (bytesToDiscard + sizeof(float) - 1) / sizeof(float));

            try
            {
                var bytesRead = Bass.ChannelGetData(_stream, preRoll, bytesToDiscard);

                if (bytesRead != bytesToDiscard)
                    throw new InvalidOperationException(
                        $"Could not pre-roll spectrogram chunk {chunkIndex}: {Bass.LastError}");
            }
            finally
            {
                ArrayPool<float>.Shared.Return(preRoll);
            }
        }

        private void DisposeDecoder()
        {
            if (_stream == 0)
                return;

            Bass.StreamFree(_stream);
            _stream = 0;
        }

        private static float Linear(float x) => x;

        private static float Mel(float frequency) => 2595 * MathF.Log10(1 + frequency / 700);

        private static float Erb1(float frequency)
            => 6.23f * frequency * frequency / 1000 / 1000 + 93.39f * frequency / 1000 + 28.52f;

        private static float Erb2(float frequency) => 24.7f * (4.37f * frequency / 1000 + 1);

        private static float Sigmoid(float x) => x < 0.2f ? 0 : (MathF.Tanh(x * 2 - 1) + 1) / 2;
    }

    internal sealed class EditorPlayfieldSpectrogramChunk : IDisposable
    {
        public Color[] Pixels { get; }
        public int Width { get; }
        public int Height { get; }
        public double StartTime { get; }
        public double Length { get; }

        private bool _isDisposed;

        public EditorPlayfieldSpectrogramChunk(Color[] pixels, int width, int height, double startTime, double length)
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
