using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;
using ManagedBass.Fx;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Quaver.Config;
using Quaver.Logging;
using Configuration = Quaver.Config.Configuration;

namespace Quaver.Audio
{
    /// <summary>
    ///     Manages all of the songs for the game. 
    /// </summary>
    internal static class SongManager
    {
        /// <summary>
        ///     The current audio stream
        /// </summary>
        internal static int AudioStream { get; set; }

        /// <summary>
        ///     The position of the current song
        /// </summary>
        internal static double Position { get => GetAudioPosition(); }

        /// <summary>
        ///     The length of the current song
        /// </summary>
        internal static double Length { get => GetAudioLength(); }

        /// <summary>
        ///     Loads up a song to be ready to be played.
        /// </summary>
        /// <param name="path"></param>
        internal static void Load()
        {
            // Make sure BASS only gets initialized one time.
            if (!GameBase.BassInitialized)
            {
                Bass.Init();
                GameBase.BassInitialized = true;
            }

            // Free the previous stream
            Bass.StreamFree(AudioStream);

            // Find the path of the current beatmap.
            var path = $"{Configuration.SongDirectory}/{GameBase.SelectedBeatmap.Directory}/{GameBase.SelectedBeatmap.AudioPath}";

            // Don't bother loading the stream if the audio file doesn't exist.
            if (!File.Exists(path))
            {
                AudioStream = 0;
                return;
            }

            // Load up the stream and have it AutoFree
            AudioStream = Bass.CreateStream(path, Flags: BassFlags.Decode);
            Bass.ChannelAddFlag(AudioStream, BassFlags.AutoFree);
        }

        /// <summary>
        ///     Plays the currently loaded track
        /// </summary>
        internal static void Play(bool previewTime = false)
        {
            if (AudioStream == 0)
                return;

            // Set Preview Time if specified
            if (previewTime)
                Bass.ChannelSetPosition(AudioStream, Bass.ChannelSeconds2Bytes(AudioStream, (GameBase.SelectedBeatmap.AudioPreviewTime + 200) / 1000));

            // Set Tempo & Pitch
            AudioStream = BassFx.TempoCreate(AudioStream, BassFlags.FxFreeSource);
            ChangeSongSpeed();

            // Set Volume
            SetSongVolume();
            ChangeMasterVolume();

            Bass.ChannelPlay(AudioStream);
        }

        /// <summary>
        ///     Pauses the current audio stream
        /// </summary>
        internal static void Pause()
        {
            if (AudioStream == 0 && Bass.ChannelIsActive(AudioStream) != PlaybackState.Playing)
                return;

            Bass.ChannelPause(AudioStream);
            Logger.Log($"Audio Stream {AudioStream} has been paused.", Color.Cyan);
        }

        /// <summary>
        ///     Resumes the current audio stream if it is paused.
        /// </summary>
        internal static void Resume()
        {
            if (AudioStream == 0 && Bass.ChannelIsActive(AudioStream) != PlaybackState.Paused)
                return;

            Bass.ChannelPlay(AudioStream);
            Logger.Log($"Audio Stream {AudioStream} has been resumed.", Color.Cyan);
        }

        /// <summary>
        ///     Completely stops the current audio stream and frees any resources it took.
        /// </summary>
        internal static void Stop()
        {
            if (AudioStream == 0 && Bass.ChannelIsActive(AudioStream) != PlaybackState.Stopped)
                return;

            // Completely stop the stream and free its resources
            Bass.ChannelStop(AudioStream);
            Bass.StreamFree(AudioStream);
        }

        /// <summary>
        ///     Toggles whether or not we want the song to be pitched or not.
        /// </summary>
        internal static void ToggleSongPitch()
        {
            if (!Configuration.Pitched)
            {
                Bass.ChannelSetAttribute(AudioStream, ChannelAttribute.Pitch, Math.Log(Math.Pow(GameBase.GameClock, 12), 2));
                Configuration.Pitched = true;
            }
            else
            {
                Bass.ChannelSetAttribute(AudioStream, ChannelAttribute.Pitch, 0);
                Configuration.Pitched = false;
            }
        }

        /// <summary>
        ///     Gets the current audio position of the stream in milliseconds.
        ///     Returns 0.0 if the stream is 0
        /// </summary>
        /// <returns></returns>
        private static double GetAudioPosition()
        {
            return (AudioStream == 0) ? 0.0f : Bass.ChannelBytes2Seconds(AudioStream, Bass.ChannelGetPosition(AudioStream)) * 1000;
        }

        /// <summary>
        ///     Returns the length of the audio stream.
        /// </summary>
        /// <returns></returns>
        private static double GetAudioLength()
        {
            return Bass.ChannelBytes2Seconds(AudioStream, Bass.ChannelGetLength(AudioStream)) * 1000;
        }

        /// <summary>
        ///     Skips to a defined position in the audio.
        /// </summary>
        internal static void SkipTo(double position)
        {
            if (AudioStream == 0)
                return;

            Bass.ChannelSetPosition(AudioStream, Bass.ChannelSeconds2Bytes(AudioStream, position / 1000));
        }

        /// <summary>
        ///     Changes the song's speed and pitch based on the current game clock
        /// </summary>
        internal static void ChangeSongSpeed()
        {
            Bass.ChannelSetAttribute(AudioStream, ChannelAttribute.Tempo, GameBase.GameClock * 100 - 100);

            // Set new pitch if any
            if (Configuration.Pitched)
                Bass.ChannelSetAttribute(AudioStream, ChannelAttribute.Pitch, Math.Log(Math.Pow(GameBase.GameClock, 12), 2));
        }

        /// <summary>
        ///     Sets the song's volume to that of the config
        /// </summary>
        private static void SetSongVolume()
        {
            var volume = (float)Configuration.VolumeMusic / 100;

            Bass.ChannelSetAttribute(AudioStream, ChannelAttribute.Volume, volume);
        }

        /// <summary>
        ///     Changes the master volume of all streams
        /// </summary>
        internal static void ChangeMasterVolume()
        {
            Bass.GlobalStreamVolume = Configuration.VolumeGlobal * 100;
        }
    }
}
