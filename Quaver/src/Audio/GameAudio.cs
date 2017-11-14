using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ManagedBass;
using ManagedBass.Fx;
using Microsoft.Xna.Framework;
using Quaver.Logging;
using Configuration = Quaver.Config.Configuration;

namespace Quaver.Audio
{
    internal class GameAudio
    {
        /// <summary>
        ///     The current Audio Stream
        /// </summary>
        protected int Stream { get; set; }

        /// <summary>
        ///     The path of the audio stream - used as temporary variables of hitsounds.
        ///     to be reloaded.
        /// </summary>
        protected string Path { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        public GameAudio(){}

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
        protected void LoadAudioStream(string filePath)
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
                AudioHandler.GlobalAudioStreams.Add(Stream);
                Stream = stream;
            }
                
            Bass.ChannelAddFlag(Stream, BassFlags.AutoFree);
        }

        /// <summary>
        /// Plays the current audio stream at a given preview time, rate, and pitch if specified.
        /// </summary>
        internal virtual void Play(double previewTime = 0, float playbackRate = 1.0f, bool pitch = false)
        {
            if (Stream == 0)
                return;

            // Set the position to play the song at 
            Bass.ChannelSetPosition(Stream, Bass.ChannelSeconds2Bytes(Stream, previewTime / 1000));

            // Set Tempo
            Stream = BassFx.TempoCreate(Stream, BassFlags.FxFreeSource);
            ChangeSongSpeed();

            // Set Pitch if necessary.
            if (pitch)
                Bass.ChannelSetAttribute(Stream, ChannelAttribute.Pitch, Math.Log(Math.Pow(playbackRate, 12), 2));

            // Change the audio volume to that of what is in the config file.
            ChangeAudioVolume();
            AudioHandler.ChangeMasterVolume();

            // Play the stream and reload the audio stream
            Bass.ChannelPlay(Stream);
        }

        /// <summary>
        ///     Pauses the current audio stream
        /// </summary>
        internal void Pause()
        {
            if (Stream == 0 && Bass.ChannelIsActive(Stream) != PlaybackState.Playing)
                return;

            Bass.ChannelPause(Stream);
            Logger.Log($"Audio Stream {Stream} has been paused.", Color.Cyan);
        }

        /// <summary>
        ///     Resumes the current audio stream if it is paused.
        /// </summary>
        internal void Resume()
        {
            if (Stream == 0 && Bass.ChannelIsActive(Stream) != PlaybackState.Paused)
                return;

            Bass.ChannelPlay(Stream);
            Logger.Log($"Audio Stream {Stream} has been resumed.", Color.Cyan);
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
        internal virtual void ChangeAudioVolume()
        {
            var volume = (float) Configuration.VolumeMusic / 100;

            Bass.ChannelSetAttribute(Stream, ChannelAttribute.Volume, volume);
        }

        /// <summary>
        ///     Changes the song's speed based on the current game clock
        /// </summary>
        internal void ChangeSongSpeed()
        {
            Bass.ChannelSetAttribute(Stream, ChannelAttribute.Tempo, GameBase.GameClock * 100 - 100);
        }
    }
}