/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using System.IO;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Wobble.Audio.Samples;

namespace Quaver.Shared.Audio
{
    public static class CustomAudioSampleCache
    {
        /// <summary>
        ///     MD5 hash of the map where the sound samples are from.
        /// </summary>
        private static string MapMd5 { get; set; }

        /// <summary>
        ///     The cached audio samples.
        /// </summary>
        private static List<GameplayAudioSample> Samples { get; set; } = new List<GameplayAudioSample>();

        /// <summary>
        ///     Currently playing channels.
        /// </summary>
        private static List<AudioSampleChannel> Channels { get; set; } = new List<AudioSampleChannel>();

        /// <summary>
        ///     Loads audio samples for the specified map into the cache.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="md5"></param>
        public static void LoadSamples(Map map, string md5)
        {
            if (map.Qua == null)
                return;

            // Always clean up the left-over channels.
            StopAll();

            // If the MD5 is the same, no need to re-load the samples.
            if (MapMd5 == md5)
                return;

            MapMd5 = map.Md5Checksum;

            foreach (var sample in Samples)
                sample.Dispose();

            Samples = new List<GameplayAudioSample>();
            foreach (var info in map.Qua.CustomAudioSamples)
            {
                // If the path is missing an extension or the file doesn't exist, we need to try some other extensions
                // for compatibility with osu!.
                var pathWithoutExt = info.Path;
                var extensions = new List<string> { "wav", "ogg", "mp3" };

                var dotIndex = info.Path.LastIndexOf('.');
                if (dotIndex > 0)
                {
                    pathWithoutExt = info.Path.Substring(0, dotIndex);
                    extensions.Insert(0, info.Path.Substring(dotIndex + 1));
                }
                else
                {
                    extensions.Insert(0, "");
                }

                var found = false;
                foreach (var ext in extensions)
                {
                    try
                    {
                        Samples.Add(new GameplayAudioSample(new AudioSample(MapManager.GetCustomAudioSamplePath(
                            map, pathWithoutExt + '.' + ext)), info.UnaffectedByRate));
                        found = true;
                        break;
                    }
                    catch (FileNotFoundException)
                    {
                        // Ignored.
                    }
                }

                // If none of the filenames worked, create a silent sample.
                if (!found)
                {
                    // Applying rate to an empty sample results in a crash.
                    Samples.Add(new GameplayAudioSample(new AudioSample(), true));
                }
            }
        }

        /// <summary>
        ///     Plays the sample for the given index.
        /// </summary>
        /// <param name="index">Index of a sample to play, same as into the Qua.CustomAudioSamples array.</param>
        /// <param name="volume">Volume between 0 and 100.</param>
        public static void Play(int index, int volume = 100)
        {
            if (index < 0 || index >= Samples.Count)
                return;

            var sample = Samples[index];
            var channel = sample.Sample.CreateChannel(
                ConfigManager.Pitched.Value, sample.UnaffectedByRate ? 1f : AudioEngine.Track.Rate);
            channel.Volume *= volume / 100f;
            channel.Play();

            Channels.Add(channel);
        }

        /// <summary>
        ///     Pauses all playing samples.
        /// </summary>
        public static void PauseAll()
        {
            for (var i = Channels.Count - 1; i >= 0; i--)
            {
                var channel = Channels[i];

                channel.Pause();

                // Remove channels that have finished playing.
                if (channel.IsStopped)
                    Channels.RemoveAt(i);
            }
        }

        /// <summary>
        ///     Resumes all samples.
        /// </summary>
        public static void ResumeAll()
        {
            foreach (var channel in Channels)
                channel.Play();
        }

        /// <summary>
        ///     Stops and frees all playing samples without the ability to resume them.
        /// </summary>
        public static void StopAll()
        {
            Channels.ForEach(x => x.Stop());
            Channels.Clear();
        }

        public static void Dispose()
        {
            StopAll();
            Samples.ForEach(x => x.Dispose());
            Samples.Clear();
            MapMd5 = null;
        }
    }
}