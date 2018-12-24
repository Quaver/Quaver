/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.IO;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Modifiers;
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
        public static AudioTrack Track { get; internal set; }

        /// <summary>
        ///     The map the loaded AudioTrack is for.
        /// </summary>
        public static Map Map { get; private set; }

        /// <summary>
        ///     Loads the track for the currently selected map.
        /// </summary>
        public static void LoadCurrentTrack()
        {
            Map = MapManager.Selected.Value;

            if (Track != null && !Track.IsDisposed)
                Track.Dispose();

            Track = new AudioTrack(MapManager.CurrentAudioPath)
            {
                Volume = ConfigManager.VolumeMusic.Value,
                Rate = ModHelper.GetRateFromMods(ModManager.Mods),
            };

            Track.ToggleRatePitching(ConfigManager.Pitched.Value);
        }

        /// <summary>
        ///     Plays the track at its preview time.
        /// </summary>
        public static void PlaySelectedTrackAtPreview()
        {
            try
            {
                LoadCurrentTrack();
                Track.Seek(MapManager.Selected.Value.AudioPreviewTime);
                Track.Play();
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
            if (Track == null || Track.IsDisposed || Track.IsStopped)
                throw new AudioEngineException("Cannot seek to nearest snap if a track isn't loaded");

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            // Get the current timing point
            var point = map.GetTimingPointAt(Track.Time);

            // Get the amount of milliseconds that each snap takes in the beat.
            var snapTimePerBeat = 60000 / point.Bpm / snap;

            // The point in the music that we want to snap to pre-rounding.
            double pointToSnap;

            switch (direction)
            {
                case Direction.Forward:
                    pointToSnap = Track.Time + snapTimePerBeat;
                    break;
                case Direction.Backward:
                    pointToSnap = Track.Time - snapTimePerBeat;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            // Snap the value and seek to it.
            var seekTime = Math.Round((pointToSnap - point.StartTime) / snapTimePerBeat) * snapTimePerBeat + point.StartTime;
            Track.Seek(seekTime);
        }
    }
}
