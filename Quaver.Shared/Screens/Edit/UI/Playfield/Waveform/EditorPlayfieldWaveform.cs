using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Audio.Tracks;
using Wobble.Graphics;
using ManagedBass;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Scheduling;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Waveform
{
    public class EditorPlayfieldWaveform : Container
    {
        /// <summary>
        ///     The currently cached waveform (if one exists)
        ///     Used to speed up load times when exiting, re-etnering editor (like play testing)
        /// </summary>
        private static Tuple<string, List<EditorPlayfieldWaveformSlice>> CachedWaveform { get; set; }

        private List<EditorPlayfieldWaveformSlice> Slices { get; set; }

        private List<EditorPlayfieldWaveformSlice> VisibleSlices { get; }

        private EditorPlayfield Playfield { get; }

        private float[] TrackData { get; set; }

        private long TrackByteLength { get; set; }

        private double TrackLengthMilliSeconds { get; set; }

        private int SliceSize { get; set; }

        private int Stream { get; set; }

        public EditorPlayfieldWaveform(EditorPlayfield playfield)
        {
            Playfield = playfield;

            Slices = new List<EditorPlayfieldWaveformSlice>();
            VisibleSlices = new List<EditorPlayfieldWaveformSlice>();

            GenerateWaveform();
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

            var amount = Math.Max(3, (int)(2.5f / Playfield.TrackSpeed + 0.5f));

            for (var i = 0; i < amount; i++)
                TryDrawSlice(index + (i - amount / 2), gameTime);
        }

        /// <summary>
        /// </summary>
        public void GenerateWaveform()
        {
            SliceSize = (int) Playfield.Height;
            GenerateTrackData();

            if (CachedWaveform != null)
            {
                // Used the cached waveform because it is the same map as the last editor session
                if (CachedWaveform.Item1 == MapManager.GetAudioPath(MapManager.Selected?.Value))
                {
                    Slices = CachedWaveform.Item2;
                    Slices.ForEach(x => x.UpdatePlayfield(Playfield));
                    Bass.StreamFree(Stream);
                    return;
                }

                // Destroy the cached waveform
                CachedWaveform.Item2.ForEach(x =>x.Destroy());
            }

            var tempSlices = new List<EditorPlayfieldWaveformSlice>();

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
                tempSlices.Add(slice);
            }

            Slices = tempSlices;
            CachedWaveform = new Tuple<string, List<EditorPlayfieldWaveformSlice>>(MapManager.GetAudioPath(MapManager.Selected?.Value), Slices);

            Bass.StreamFree(Stream);
        }

        /// <summary>
        /// </summary>
        private void GenerateTrackData()
        {
            const BassFlags flags = BassFlags.Decode | BassFlags.Float;

            Stream = Bass.CreateStream(((AudioTrack)Audio.AudioEngine.Track).OriginalFilePath, 0, 0, flags);

            TrackByteLength = Bass.ChannelGetLength(Stream);
            TrackData = new float[TrackByteLength / sizeof(float)];

            TrackByteLength = Bass.ChannelGetData(Stream, TrackData, (int)TrackByteLength);

            TrackLengthMilliSeconds = Bass.ChannelBytes2Seconds(Stream, TrackByteLength) * 1000.0;
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

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (CachedWaveform.Item2 == Slices)
            {
                base.Destroy();
                return;
            }

            foreach (var slice in Slices)
                slice.Destroy();

            Slices.Clear();
        }
    }
}