using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Wobble.Audio.Tracks;
using Wobble.Graphics;
using ManagedBass;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Scheduling;
using Wobble.Logging;
using ManagedBass.Fx;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Edit.UI.Playfield.Waveform;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Spectrogram
{
    public class EditorPlayfieldSpectrogram : Container
    {
        private List<EditorPlayfieldSpectrogramSlice> Slices { get; set; }

        private List<EditorPlayfieldSpectrogramSlice> VisibleSlices { get; }

        private EditorPlayfield Playfield { get; }

        private float[,] TrackData { get; set; }

        private long TrackByteLength { get; set; }

        private double TrackLengthMilliSeconds { get; set; }

        private int FftPerSlice { get; set; }

        private int Stream { get; set; }

        public int FftCount { get; }
        
        public int FftFlag { get; }
        public int FftResultCount { get; set; }

        private int FftRoundsTaken { get; set; }

        private int BytesReadPerFft { get; set; }

        private CancellationToken Token { get; }

        public EditorPlayfieldSpectrogram(EditorPlayfield playfield, CancellationToken token)
        {
            Playfield = playfield;
            Token = token;
            FftCount = ConfigManager.EditorSpectrogramFftSize.Value;
            FftFlag = (int)GetFftDataFlag(FftCount);

            Slices = new List<EditorPlayfieldSpectrogramSlice>();
            VisibleSlices = new List<EditorPlayfieldSpectrogramSlice>();

            GenerateWaveform();
            CheckCancellationToken();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Slices.Count > 0)
            {
                foreach (var slice in Slices)
                    slice.Update(gameTime);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            var index = (int)(Audio.AudioEngine.Track.Time / TrackLengthMilliSeconds * Slices.Count);

            var amount = Math.Max(6, (int)(2.5f / Playfield.TrackSpeed + 0.5f));

            for (var i = 0; i < amount; i++)
                TryDrawSlice(index + (i - amount / 2), gameTime);
        }

        /// <summary>
        /// </summary>
        public void GenerateWaveform()
        {
            FftPerSlice = (int)Playfield.Height;
            GenerateTrackData();

            var tempSlices = new List<EditorPlayfieldSpectrogramSlice>();
            var millisecondPerFft = Bass.ChannelBytes2Seconds(Stream, BytesReadPerFft) * 1000;
            var millisecondPerSlice = FftPerSlice * millisecondPerFft;
            var sampleRate = Bass.ChannelGetInfo(Stream).Frequency;

            for (var fftRound = 0; fftRound < FftRoundsTaken; fftRound += FftPerSlice)
            {
                var t = (int)(fftRound * millisecondPerFft);
                var trackSliceData = new float[FftPerSlice, FftResultCount];

                for (var y = 0; y < FftPerSlice; y++)
                {
                    var currentFftIndex = fftRound + y;
                    if (currentFftIndex >= TrackData.GetLength(0))
                        break;
                    for (var x = 0; x < FftResultCount; x++)
                    {
                        trackSliceData[y, x] = TrackData[currentFftIndex, x];
                    }
                }

                var slice = new EditorPlayfieldSpectrogramSlice(this, Playfield, (float)millisecondPerSlice, FftPerSlice,
                    trackSliceData, t, sampleRate);
                tempSlices.Add(slice);
            }

            Slices = tempSlices;
            Bass.StreamFree(Stream);
        }

        /// <summary>
        /// </summary>
        private void GenerateTrackData()
        {
            const BassFlags flags = BassFlags.Decode | BassFlags.Float;

            Stream = Bass.CreateStream(((AudioTrack)Audio.AudioEngine.Track).OriginalFilePath, 0, 0, flags);

            TrackByteLength = Bass.ChannelGetLength(Stream);
            FftResultCount = FftCount;
            BytesReadPerFft = sizeof(float) * FftResultCount * Bass.ChannelGetInfo(Stream).Channels * 2;
            TrackData = new float[TrackByteLength / BytesReadPerFft + 1, FftResultCount];

            var trackDataFft = new float[FftResultCount];
            FftRoundsTaken = 0;
            
            while (Bass.ChannelGetData(Stream, trackDataFft, FftFlag | (int)DataFlags.FFTRemoveDC) > 0)
            {
                for (var i = 0; i < FftResultCount; i++)
                {
                    TrackData[FftRoundsTaken, i] = trackDataFft[i];
                }
                FftRoundsTaken++;
            }

            TrackByteLength = Bass.ChannelGetLength(Stream);
            TrackLengthMilliSeconds = Bass.ChannelBytes2Seconds(Stream, TrackByteLength) * 1000.0;
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

        /// <summary>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="gameTime"></param>
        private void TryDrawSlice(int index, GameTime gameTime)
        {
            if (index >= 0 && index < Slices.Count)
                Slices[index]?.Draw(gameTime);
        }

        private void CheckCancellationToken()
        {
            if (!Token.IsCancellationRequested)
                return;

            Destroy();
        }

        /// <summary>
        /// </summary>
        public override void Destroy() => DisposeWaveform();

        /// <summary>
        /// </summary>
        public void DisposeWaveform()
        {
            DisposeSlices();
            base.Destroy();
        }

        private void DisposeSlices()
        {
            foreach (var slice in Slices)
                slice.Destroy();

            Slices.Clear();
        }
    }
}