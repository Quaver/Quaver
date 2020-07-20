using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Audio.Tracks;
using Wobble.Graphics;
using ManagedBass;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Waveform
{
    public class EditorPlayfieldWaveform : Container
    {
        private List<EditorPlayfieldWaveformSlice> Slices { get; }

        private List<EditorPlayfieldWaveformSlice> VisibleSlices { get; }

        private EditorPlayfield Playfield { get; set; }

        private float[] TrackData { get; set; }

        private long TrackByteLength { get; set; }

        private double TrackLengthMilliSeconds { get; set; }

        private int SliceSize { get; set; }

        private int Stream { get; set; }

        public EditorPlayfieldWaveform()
        {
            Slices = new List<EditorPlayfieldWaveformSlice>();
            VisibleSlices = new List<EditorPlayfieldWaveformSlice>();
        }

        public void GenerateWaveform(EditorPlayfield playfield)
        {
            Playfield = playfield;

            GenerateTrackData();

            SliceSize = (int)playfield.Height;

            for (var t = 0; t < TrackLengthMilliSeconds; t += SliceSize)
            {
                var trackSliceData = new float[SliceSize, 2];

                for (var y = 0; y < SliceSize; y++)
                {
                    var timePoint = t + y;
                    var index = Bass.ChannelSeconds2Bytes(Stream, timePoint / 1000.0) / 4;

                    if (index >= TrackByteLength / sizeof(float))
                        continue;

                    trackSliceData[y, 0] = TrackData[index];
                    trackSliceData[y, 1] = TrackData[index + 1];
                }

                var slice = new EditorPlayfieldWaveformSlice(Playfield, SliceSize, trackSliceData, t);
                Slices.Add(slice);
            }

            Bass.StreamFree(Stream);
        }


        public void Draw(GameTime gameTime)
        {
            var index = (int)(Audio.AudioEngine.Track.Time / TrackLengthMilliSeconds * Slices.Count);

            var amount = Math.Max(3, (int)(2.5f / Playfield.TrackSpeed + 0.5f));

            for (var i = 0; i < amount; i++)
                TryDrawSlice(index + (i - amount / 2), gameTime);

            //keeping this if the thing above fails for some reason you never know lol (but it should work I promise)
            //TryDrawSlice(index - 2, gameTime);
            //TryDrawSlice(index - 1, gameTime);
            //TryDrawSlice(index,     gameTime);
            //TryDrawSlice(index + 1, gameTime);
            //TryDrawSlice(index + 2, gameTime);
            //TryDrawSlice(index + 3, gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var slice in Slices)
                slice.Update(gameTime);

            base.Update(gameTime);
        }

        private void GenerateTrackData()
        {
            const BassFlags flags = BassFlags.Decode | BassFlags.Float;

            Stream = Bass.CreateStream(((AudioTrack)Audio.AudioEngine.Track).OriginalFilePath, 0, 0, flags);

            TrackByteLength = Bass.ChannelGetLength(Stream);
            TrackData = new float[TrackByteLength / sizeof(float)];

            TrackByteLength = Bass.ChannelGetData(Stream, TrackData, (int)TrackByteLength);

            TrackLengthMilliSeconds = Bass.ChannelBytes2Seconds(Stream, TrackByteLength) * 1000.0;
        }

        private void TryDrawSlice(int index, GameTime gameTime)
        {
            if (index >= 0 && index < Slices.Count)
                Slices[index]?.Draw(gameTime);
        }

        public override void Destroy()
        {
            foreach (var slice in Slices)
                slice.Destroy();

            Slices.Clear();

            base.Destroy();
        }
    }
}