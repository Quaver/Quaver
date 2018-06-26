using System;
using System.IO;
using System.Linq;
using ManagedBass;
using ManagedBass.Fx;
using Microsoft.Xna.Framework.Audio;
using Quaver.Config;
using Quaver.Main;
using Quaver.Modifiers;
using Quaver.Modifiers.Mods;

namespace Quaver.Audio
{ 
    internal class AudioEngine
    {
        /// <summary>
        ///     The currently loaded audio stream
        /// </summary>
        internal static int Stream { get; private set; }

        /// <summary>
        ///     It's assumed that BASS has a delay when streams are being played. 
        ///     This offset tries to mitigate the offset, however the value is seemingly
        ///     arbitrary.
        /// </summary>
        internal static int BassDelayOffset { get; } = 15;

        /// <summary>
        ///     The length of the current audio stream in milliseconds.
        /// </summary>
        internal double Length => Bass.ChannelBytes2Seconds(Stream, Bass.ChannelGetLength(Stream)) * 1000;

        /// <summary>
        ///     The position of the current audio stream in milliseconds
        /// </summary>
        internal double Position => Bass.ChannelBytes2Seconds(Stream, Bass.ChannelGetPosition(Stream)) * 1000;

        /// <summary>
        ///     The position of the audio including frame times.
        /// </summary>
        internal double Time { get; private set; }
        
        /// <summary>
        ///     If the audio has already previously been played before.
        /// </summary>
        internal bool HasPlayed { get; set; }

        /// <summary>
        ///     Returns if the audio stream is currently pitched.
        /// </summary>
        internal bool IsPitched { get; set; }

        /// <summary>
        ///     Returns if the audio stream is currently playing.
        /// </summary>
        internal bool IsPlaying => Bass.ChannelIsActive(Stream) == PlaybackState.Playing;

        /// <summary>
        ///     Returns if the audio stream is currently paused
        /// </summary>
        internal bool IsPaused => Bass.ChannelIsActive(Stream) == PlaybackState.Paused;
        
        /// <summary>
        ///     Returns if the audio stream is currently stopped.
        /// </summary>
        internal bool IsStopped => Bass.ChannelIsActive(Stream) == PlaybackState.Stopped;

        /// <summary>
        ///     Event invoked when the audio has started playing
        /// </summary>
        internal EventHandler OnPlayed { get; set; }

        /// <summary>
        ///     Event invoked when the audio has been paused.
        /// </summary>
        internal EventHandler OnPaused { get; set; }

        /// <summary>
        ///     Event invoked when the audio has been stopped.
        /// </summary>
        internal EventHandler OnStopped { get; set; }

        /// <summary>
        ///     The master volume of all audio streams
        /// </summary>
        internal static int MasterVolume
        {
            get => Bass.GlobalStreamVolume;
            set
            {
                Bass.GlobalStreamVolume = value * 100;
                SoundEffect.MasterVolume = value / 100f;
            }
        }

        /// <summary>
        ///     The volume of the current stream.
        /// </summary>
        internal static double MusicVolume
        {
            get => Bass.ChannelGetAttribute(Stream, ChannelAttribute.Volume);
            set => Bass.ChannelSlideAttribute(Stream, ChannelAttribute.Volume, (float)(value / 100f), 50);
        }

        /// <summary>
        ///     The volume of all sound effects.
        /// </summary>
        internal static float EffectVolume => ConfigManager.VolumeEffect.Value / 100f;

        /// <summary>
        ///     The rate at which the audio stream will play at.
        /// </summary>
        internal float PlaybackRate = 1.0f;

        /// <summary>
        ///     The audio state in the previous state
        /// </summary>
        private PlaybackState PreviousState { get; set; } = PlaybackState.Stopped;

        /// <summary>
        ///     The current audio state
        /// </summary>
        internal PlaybackState State { get; private set; } = PlaybackState.Stopped;

        /// <summary>
        ///     Constructor - Intitializes BASS.
        /// </summary>
        internal AudioEngine()
        {
            if (!Bass.Init())
                throw new AudioEngineException("BASS has failed to intiailize");

            // Set volume curves to be logarithmic
            Bass.LogarithmicVolumeCurve = false;
            Bass.LogarithmicPanningCurve = true;

            // Initialize event handlers. This'll auto-change the music volume automatically when it changes.
            ConfigManager.VolumeGlobal.OnValueChanged += (o, e) => MasterVolume = e.Value;
            ConfigManager.VolumeMusic.OnValueChanged += (o, e) => MusicVolume = e.Value;
        }

        /// <summary>
        ///     Loads an AudioStream
        /// </summary>
        internal void Load()
        {
            if (Stream != 0)
                Dispose();

            if (!File.Exists(GameBase.CurrentAudioPath))
                throw new AudioEngineException($"The audio file: {GameBase.CurrentAudioPath} could not be found.");

            Stream = Bass.CreateStream(GameBase.CurrentAudioPath, Flags: BassFlags.Decode);
            Stream = BassFx.TempoCreate(Stream, BassFlags.FxFreeSource);
            Bass.ChannelAddFlag(Stream, BassFlags.AutoFree);
        }

        /// <summary>
        ///     Gets the accurate time of the song including frame times.
        /// </summary>
        /// <param name="dt"></param>
        internal void Update(double dt)
        {
            PreviousState = State;
            State = Bass.ChannelIsActive(Stream);
            
            // Emit an event when the audio has been stopped.
            if (State == PlaybackState.Stopped && (PreviousState == PlaybackState.Playing || PreviousState == PlaybackState.Paused))
                OnStopped?.Invoke(this, EventArgs.Empty);
            
            if (Stream == 0 || Bass.ChannelIsActive(Stream) == PlaybackState.Stopped)
            {
                Time = 0;
                return;
            }
                
            if (!IsPlaying)
            {
                Time = Position;
                return;
            }

            Time = (GameBase.AudioEngine.Position + (Time + dt * GameBase.AudioEngine.PlaybackRate)) / 2;
        }
   
        /// <summary>
        ///     Plays the loaded audio stream.
        /// </summary>
        /// <param name="pos">The position in the audio to play at.</param>
        /// <param name="playbackRate">The rate at which to play the audio</param>
        /// <param name="pitched">Determines if the audio played is pitched relative to its playback rate</param>
        internal void Play(int pos = 0)
        {
            if (Stream == 0)
                throw new AudioEngineException("You cannot play an audio stream when one is not loaded.");

            if (Bass.ChannelIsActive(Stream) == PlaybackState.Playing)
                throw new AudioEngineException("You cannot play an audio stream while one is already playing.");

            // Set the song position 
            ChangeSongPosition(pos);

            // Set the playback rate AND THEN toggle the pitch.
            SetPlaybackRate();
            SetPitch();

            // Set volume
            MasterVolume = ConfigManager.VolumeGlobal.Value;
            MusicVolume = ConfigManager.VolumeMusic.Value;

            Bass.ChannelPlay(Stream);
            HasPlayed = true;
            
            OnPlayed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Plays a sound effect.
        /// </summary>
        internal void PlaySoundEffect(SoundEffect sfx, float pitch = 0)
        {
            sfx.Play(EffectVolume, pitch, 0);
        }

        /// <summary>
        ///     Pauses the loaded audio stream
        /// </summary>
        internal void Pause()
        {
            if (Stream == 0)
                throw new AudioEngineException("You cannot pause an audio stream if one is not currently loaded.");

            if (Bass.ChannelIsActive(Stream) != PlaybackState.Playing)
                throw new AudioEngineException("You cannot pause an audio stream if one is currently not playing.");

            Bass.ChannelPause(Stream);
            
            OnPaused?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Resumes the audio stream
        /// </summary>
        internal void Resume()
        {
           if (Stream == 0)
                throw new AudioEngineException("You cannot resume an audio stream if one is not loaded.");

           if (Bass.ChannelIsActive(Stream) != PlaybackState.Paused)
                throw new AudioEngineException("You cannot resume an audio stream if one is currently not paused.");

            Bass.ChannelPlay(Stream);
        }

        /// <summary>
        ///     Stops the current audio stream and disposes all the resources
        /// </summary>
        internal void Stop()
        {
            if (Stream == 0)
                throw new AudioEngineException("You cannot stop an audio stream if one is currently not loaded.");

            Bass.ChannelStop(Stream);
            Bass.StreamFree(Stream);

            Stream = 0;
            HasPlayed = false;
        }

        /// <summary>
        ///     Toggles the pitching for the audio stream
        /// </summary>
        internal void SetPitch()
        {
            if (ConfigManager.Pitched.Value)
                Bass.ChannelSetAttribute(Stream, ChannelAttribute.Pitch, Math.Log(Math.Pow(PlaybackRate, 12), 2));
            else
                Bass.ChannelSetAttribute(Stream, ChannelAttribute.Pitch, 0);
        }

        /// <summary>
        ///     Toggles the pitch of the audio stream on/off.
        /// </summary>
        internal void TogglePitch()
        {
            ConfigManager.Pitched.Value = !ConfigManager.Pitched.Value;
            SetPitch();
        }

        /// <summary>
        ///     Changes the song position at a given point in milliseconds
        /// </summary>
        /// <param name="pos"></param>
        internal void ChangeSongPosition(double pos)
        {
            if (Stream == 0)
                throw new AudioEngineException("You cannot change the song's position if one isn't loaded!");

            if (pos > Length)
                throw new AudioEngineException("You cannot play an audio stream at a position greater than its length");

            if (pos > 0)
                Bass.ChannelSetPosition(Stream, Bass.ChannelSeconds2Bytes(Stream, pos / 1000d));
        }

        /// <summary>
        ///     Sets the playback rate based on the current game's clock.
        /// </summary>
        internal void SetPlaybackRate(bool checkInconsistencies = true)
        {
            if (checkInconsistencies)
                ModManager.CheckModInconsistencies();
            
            Bass.ChannelSetAttribute(Stream, ChannelAttribute.Tempo, PlaybackRate * 100 - 100);
            SetPitch();
        }

        /// <summary>
        ///     Reloads the audio stream
        /// </summary>
        internal void ReloadStream()
        {
            if (Stream != 0)
                Dispose();

            Load();
        }

        /// <summary>
        ///     Stops, disposes and frees the current audio stream.
        /// </summary>
        private void Dispose()
        {
            if (Stream != 0)
            {
                try
                {
                    Stop();
                }
                catch (Exception e)
                {
                    
                }
            }
                
            Bass.StreamFree(Stream);
            Stream = 0;
            HasPlayed = false;
        }

        /// <summary>
        ///     Frees the loaded stream and BASS
        /// </summary>
        internal void Free()
        {
            if (Stream != 0)
            {
                Bass.StreamFree(Stream);
                Stream = 0;
            }
                
            Bass.Free();
        }

        /// <summary>
        ///     Fades the current audio stream's volume to a given volume in a specified time frame.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="time"></param>
        internal static void Fade(float to, int time) => Bass.ChannelSlideAttribute(Stream, ChannelAttribute.Volume, to, time);
    }

    /// <inheritdoc />
    /// <summary>
    ///     An Audio Engine exception
    /// </summary>
    internal class AudioEngineException : Exception
    {
        public AudioEngineException(string message) : base(message) { }
    }
}