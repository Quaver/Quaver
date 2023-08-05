/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Scheduling;
using Wobble.Audio;
using Wobble.Audio.Tracks;
using Wobble.Graphics;

namespace Quaver.Shared.Audio
{
    public static class AudioEngine
    {
        /// <summary>
        ///     The AudioTrack for the currently selected map.
        /// </summary>
        public static IAudioTrack Track { get; internal set; }

        /// <summary>
        ///     The map the loaded AudioTrack is for.
        /// </summary>
        public static Map Map { get; set; }

        /// <summary>
        ///     Cancellation token to prevent multiple audio tracks playing at once
        /// </summary>
        private static CancellationTokenSource Source { get; set; } = new CancellationTokenSource();

        /// <summary>
        ///     Loads the track for the currently selected map.
        /// </summary>
        public static void LoadCurrentTrack(bool preview = false, int time = 300000)
        {
            Source.Cancel();
            Source.Dispose();
            Source = new CancellationTokenSource();

            Map = MapManager.Selected.Value;
            var token = Source.Token;

            try
            {
                if (Track != null && !Track.IsDisposed)
                    Track.Dispose();

                var newTrack = new AudioTrack(MapManager.CurrentAudioPath, false)
                {
                    Rate = ModHelper.GetRateFromMods(ModManager.Mods),
                };

                token.ThrowIfCancellationRequested();

                Track = newTrack;
                Track.ApplyRate(ConfigManager.Pitched.Value);
            }
            catch (OperationCanceledException e)
            {
            }
            catch (Exception e)
            {
                if (Track != null && !Track.IsDisposed)
                    Track.Dispose();

                Track = new AudioTrackVirtual(time);
            }
        }

        /// <summary>
        ///     Plays the track at its preview time.
        /// </summary>
        public static void PlaySelectedTrackAtPreview()
        {
            try
            {
                if (MapManager.Selected?.Value == null)
                    return;

                if (Track != null)
                {
                    lock (Track)
                        LoadCurrentTrack(true);
                }
                else
                {
                    LoadCurrentTrack(true);
                }

                if (Track == null)
                    return;

                lock (Track)
                {
                    Track?.Seek(MapManager.Selected.Value.AudioPreviewTime);

                    if (!Track.IsPlaying)
                        Track?.Play();
                }
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        /// <summary>
        ///     Seeks to the nearest snap(th) beat in the audio based on the
        ///     current timing point's snap.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="direction"></param>
        /// <param name="snap"></param>
        public static void SeekTrackToNearestSnap(Qua map, Direction direction, int snap)
        {
            var seekTime = GetNearestSnapTimeFromTime(map, direction, snap, Track.Time);

            if (seekTime < 0 || seekTime > Track.Length)
                return;

            Track.Seek(seekTime);
        }

        /// <summary>
        ///     Gets the nearest snap time at a given direction.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="direction"></param>
        /// <param name="snap"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <exception cref="AudioEngineException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static double GetNearestSnapTimeFromTime(Qua map, Direction direction, int snap, double time)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            // Get the current timing point
            var point = map.GetTimingPointAt(time);

            if (point == null)
                return 0;

            // Get the amount of milliseconds that each snap takes in the beat.
            var snapTimePerBeat = 60000 / point.Bpm / snap;

            // The point in the music that we want to snap to pre-rounding.
            double pointToSnap;

            switch (direction)
            {
                case Direction.Forward:
                    pointToSnap = time + snapTimePerBeat;
                    break;
                case Direction.Backward:
                    pointToSnap = time - snapTimePerBeat;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            var nearestTick = Math.Round((pointToSnap - point.StartTime) / snapTimePerBeat) * snapTimePerBeat + point.StartTime;

            if (Math.Abs(nearestTick - time) - snapTimePerBeat <= snapTimePerBeat / 2)
                return nearestTick;

            if (direction == Direction.Backward)
                return (Math.Round((pointToSnap - point.StartTime) / snapTimePerBeat) + 1) * snapTimePerBeat + point.StartTime;

            return (Math.Round((pointToSnap - point.StartTime) / snapTimePerBeat) - 1) * snapTimePerBeat + point.StartTime;
        }

        public static IAudioTrack LoadMapAudioTrack(Map map)
        {
            IAudioTrack track;

            try
            {
                track = new AudioTrack(MapManager.GetAudioPath(map), false, false);
            }
            catch (Exception)
            {
                track = new AudioTrackVirtual(map.SongLength + 5000);
            }

            return track;
        }
    }
}
