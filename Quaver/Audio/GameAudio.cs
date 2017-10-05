using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using ManagedBass;
using Configuration = Quaver.Config.Configuration;

namespace Quaver.Audio
{
    internal class GameAudio
    {
        public int Stream { get; set; }

        /// <summary>
        /// Constructor - We check if the file path is correct, and if it is, we try to load the audio stream
        /// </summary>
        /// <param name="filePath"></param>
        public GameAudio(string filePath)
        {
            // If the file doesn't exist, the stream is set to 0, which means it cannot be played.
            if (!File.Exists(filePath))
            {
                Stream = 0;
                return;
            }

            // Otherwise, we are going to load the audio stream
            LoadAudioStream(filePath);
        }

        /// <summary>
        ///     Loads an audio stream, called automatically upon instantiation
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadAudioStream(string filePath)
        {
            if (Bass.Init())
            {
                var stream = Bass.CreateStream(filePath);

                if (stream != 0)
                    Stream = stream;

                Console.WriteLine("[AUDIO ENGINE] Error: {0}", Bass.LastError);
                return;
            }

            Stream = 0;
        }

        /// <summary>
        /// Plays the current audio stream at a given preview time if specified.
        /// </summary>
        internal void Play(double previewTime = 0)
        {
            if (Stream == 0 && Bass.ChannelIsActive(Stream) != PlaybackState.Stopped)
                return;

            // Set the volume of the track, to that of what is in the config.
            Bass.Volume = (float)Configuration.VolumeGlobal / 100;

            // Set the position to play the song at 
            Bass.ChannelSetPosition(Stream, Bass.ChannelSeconds2Bytes(Stream, previewTime / 1000));

            // Start playing!
            Bass.ChannelPlay(Stream);
            Console.WriteLine($"[AUDIO ENGINE] Audio Stream: {Stream} has started playing at position: {previewTime}");
        }

        /// <summary>
        ///     Pauses the current audio stream
        /// </summary>
        internal void Pause()
        {
            if (Stream == 0 && Bass.ChannelIsActive(Stream) != PlaybackState.Playing)
                return;

            Bass.ChannelPause(Stream);
            Console.WriteLine($"[AUDIO ENGINE] Audio Stream: {Stream} has been paused.");
        }

        /// <summary>
        ///     Resumes the current audio stream if it is paused.
        /// </summary>
        internal void Resume()
        {
            if (Stream == 0 && Bass.ChannelIsActive(Stream) != PlaybackState.Paused)
                return;

            Bass.ChannelPlay(Stream);
            Console.WriteLine($"[AUDIO ENGINE] Audio Stream: {Stream} has been resumed.");
        }
    }
}