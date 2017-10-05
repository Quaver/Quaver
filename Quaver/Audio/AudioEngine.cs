using System;
using System.Security.Cryptography;
using ManagedBass;
using Configuration = Quaver.Config.Configuration;

namespace Quaver.Audio
{
    internal class AudioImplementation
    {
        /// <summary>
        ///     Loads an audio stream
        ///     and returns it
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static int LoadAudioStream(string filePath)
        {
            if (Bass.Init())
            {
                var stream = Bass.CreateStream(filePath);

                if (stream != 0)
                    return stream;

                Console.WriteLine("[AUDIO ENGINE] Error: {0}", Bass.LastError);
            }
            return 0;
        }

        /// <summary>
        ///     Plays an audio stream
        /// </summary>
        /// <param name="stream"></param>
        internal static void PlayAudioStream(int stream, double previewTime = 0)
        {
            if (stream != 0 && Bass.ChannelIsActive(stream) == PlaybackState.Stopped)
            {
                Bass.Volume = (float)Configuration.VolumeGlobal / 100;
                Bass.ChannelSetPosition(stream, Bass.ChannelSeconds2Bytes(stream, previewTime / 1000));
                Bass.ChannelPlay(stream);
                Console.WriteLine($"[AUDIO ENGINE] Audio Stream: {stream} has started playing at position: {previewTime}");
            }
        }

        /// <summary>
        ///     Responsible for pausing an audio stream.
        /// </summary>
        /// <param name="stream"></param>
        internal static void PauseAudioStream(int stream)
        {
            if (stream != 0 && Bass.ChannelIsActive(stream) == PlaybackState.Playing)
            {
                Bass.ChannelPause(stream);
                Console.WriteLine($"[AUDIO ENGINE] Audio Stream: {stream} has been paused.");
            }
        }

        internal static void ResumeAudioStream(int stream)
        {
            if (stream != 0 && Bass.ChannelIsActive(stream) == PlaybackState.Paused)
            {
                Bass.ChannelPlay(stream);
                Console.WriteLine($"[AUDIO ENGINE] Audio Stream: {stream} has been resumed.");
            }
        }
    }
}