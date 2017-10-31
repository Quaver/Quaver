using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ManagedBass;
using ManagedBass.Fx;
using Quaver.Main;
using Configuration = Quaver.Config.Configuration;

namespace Quaver.Audio
{
    internal class GameAudio
    {
        public int Stream { get; set; }
        public static List<int> Streams { get; set; } = new List<int>();
        private bool isEffect { get; }
        private string Path { get; set; }

        /// <summary>
        /// Constructor - We check if the file path is correct, and if it is, we try to load the audio stream
        /// </summary>
        /// <param name="filePath"></param>
        public GameAudio(string filePath, bool effect = false)
        {
            // If the file doesn't exist, the stream is set to 0, which means it cannot be played.
            if (!File.Exists(filePath))
            {
                Stream = 0;
                return;
            }

            if (effect)
                isEffect = true;

            // Otherwise, we are going to load the audio stream
            LoadAudioStream(filePath);
        }

        public GameAudio(Stream stream, bool effect = false)
        {
            if (effect)
                isEffect = true;

            try
            {
                byte[] buffer = new byte[8 * 1024];

                var path = Configuration.DataDirectory + "/temp.mp3";
                var file = File.Create(path);
                int len;
                while ((len = stream.Read(buffer, 0, buffer.Length)) > 0)
                {         
                    file.Write(buffer, 0, len);
                }

                file.Close();

                Path = path;
                LoadAudioStream(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     Loads an audio stream, called automatically upon instantiation
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadAudioStream(string filePath)
        {
            // Make sure BASS only gets initialized one time.
            if (!GameBase.BassInitialized)
            {
                Bass.Init();
                GameBase.BassInitialized = true;
            }
                
            // Cr3eate the stream
            var stream = Bass.CreateStream(filePath, Flags: BassFlags.Decode);

            if (stream != 0)
            {
                Streams.Add(Stream);
                Stream = stream;
            }
                
            Bass.ChannelAddFlag(Stream, BassFlags.AutoFree);
        }

        /// <summary>
        /// Plays the current audio stream at a given preview time, rate, and pitch if specified.
        /// </summary>
        internal void Play(double previewTime = 0, float playbackRate = 1.0f, bool pitch = false)
        {
            if (Stream == 0)
                return;

            // Set the position to play the song at 
            Bass.ChannelSetPosition(Stream, Bass.ChannelSeconds2Bytes(Stream, previewTime / 1000));

            // Set Tempo
            Stream = BassFx.TempoCreate(Stream, BassFlags.FxFreeSource);
            Bass.ChannelSetAttribute(Stream, ChannelAttribute.Tempo, playbackRate * 100 - 100);

            // Set Pitch if necessary.
            if (pitch)
                Bass.ChannelSetAttribute(Stream, ChannelAttribute.Pitch, Math.Log(Math.Pow(playbackRate, 12), 2));

            // Change the audio volume to that of what is in the config file.
            ChangeAudioVolume();
            ChangeMasterVolume();

            // Play the stream and reload the audio stream
            Bass.ChannelPlay(Stream);
            LoadAudioStream(Path);
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

        /// <summary>
        ///     Completely stops the current audio stream and frees any resources it took.
        /// </summary>
        internal void Stop()
        {
            if (Stream == 0 && Bass.ChannelIsActive(Stream) != PlaybackState.Stopped)
                return;

            // Completely stop the stream and free its resources
            Bass.ChannelStop(Stream);
            Bass.StreamFree(Stream);
        }

        /// <summary>
        ///     Gets the current audio position of the stream in milliseconds.
        ///     Returns 0.0 if the stream is 0
        /// </summary>
        /// <returns></returns>
        internal double GetAudioPosition()
        {
            return (Stream == 0) ? 0.0f : Bass.ChannelBytes2Seconds(Stream, Bass.ChannelGetPosition(Stream)) * 1000;
        }

        /// <summary>
        ///     Returns the length of the audio stream.
        /// </summary>
        /// <returns></returns>
        internal double GetAudioLength()
        {
            return Bass.ChannelBytes2Seconds(Stream, Bass.ChannelGetLength(Stream)) * 1000;
        }

        /// <summary>
        ///     Skips to a defined position in the audio.
        /// </summary>
        internal void SkipTo(double position)
        {
            if (Stream == 0)
                return;

            Bass.ChannelSetPosition(Stream, Bass.ChannelSeconds2Bytes(Stream, position / 1000));
        }

        /// <summary>
        ///     Sets the audio volume to that of what is in the config file, depending on if it is an effect or not.
        /// </summary>
        internal void ChangeAudioVolume()
        {
            var volume = (isEffect) ? (float) Configuration.VolumeEffect / 100 : (float) Configuration.VolumeMusic / 100;

            Bass.ChannelSetAttribute(Stream, ChannelAttribute.Volume, volume);
        }

        /// <summary>
        ///     Changes the master volume of all streams
        /// </summary>
        internal void ChangeMasterVolume()
        {
            Bass.GlobalStreamVolume = Configuration.VolumeGlobal * 100;
        }

        /// <summary>
        ///     Frees all available streams, ran continuously throughout the game.
        ///     See: QuaverGame.elapsedEventHandler
        /// </summary>
        internal static void FreeAvailableStreams()
        {
            foreach (var audioStream in Streams)
            {
                if (Bass.ChannelIsActive(audioStream) == PlaybackState.Stopped)
                    Bass.StreamFree(audioStream);
            }         
        }
    }
}