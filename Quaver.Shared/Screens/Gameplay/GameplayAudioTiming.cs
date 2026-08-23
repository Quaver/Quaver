/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
 */

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Tournament.Gameplay;
using Wobble;
using Wobble.Audio;
using Wobble.Audio.Tracks;
using Wobble.Logging;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Quaver.Shared.Screens.Gameplay
{
    public class GameplayAudioTiming
    {
        /// <summary>
        ///     Reference to the gameplay screen itself.
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <summary>
        ///     The amount of time it takes before the gameplay/song actually starts.
        /// </summary>
        public static int StartDelay { get; } = 3000;

        /// <summary>
        ///     The time in the audio/play.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        ///     Used to determine when to sync Time when SmoothAudioTiming is on.
        /// </summary>
        private double PreviousTime { get; set; }

        /// <summary>
        ///     Prevent a bad device latency report from starting a song excessively early.
        /// </summary>
        private const double MaximumAudioPrestartMs = 100;

        /// <summary>
        ///     How long the startup clock should take to correct its phase difference with the audio clock.
        /// </summary>
        private const double StartupCorrectionDurationMs = 100;

        /// <summary>
        ///     Limits startup clock correction to a subtle speed change while guaranteeing monotonic time.
        /// </summary>
        private const double MaximumStartupCorrectionRate = 0.1;

        /// <summary>
        ///     The appropriate map time to start playing the audio, accounting for output device latency.
        /// </summary>
        private double TimeToPlayAudio { get; set; }

        /// <summary>
        ///     Whether the startup clock has converged and normal audio timing can take over.
        /// </summary>
        private bool UseAudioTime { get; set; }

        /// <summary>
        ///     Largest phase difference observed while handing gameplay timing to BASS.
        /// </summary>
        private double MaximumStartupClockError { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        public GameplayAudioTiming(GameplayScreen screen)
        {
            Screen = screen;

            if (Screen.IsSongSelectPreview || Screen.UseExistingAudioTime)
            {
                UseAudioTime = true;
                Time = AudioEngine.Track?.Time ?? 0;
                return;
            }

            try
            {
                if (Screen.IsCalibratingOffset)
                    AudioEngine.Track =
                        new AudioTrack(GameBase.Game.Resources.Get($"Quaver.Resources/Maps/Offset/offset.mp3"));
                else
                {
                    AudioEngine.LoadCurrentTrack();
                    AudioEngine.Track.Rate = ModHelper.GetRateFromMods(ModManager.Mods);
                }

                if (Screen.IsPlayTesting)
                {
                    const int delay = 500;


                    if (Screen.PlayTestAudioTime < StartDelay)
                    {
                        PrepareTrackForPlayback();
                        ConfigureAudioStart();
                        Time = Screen.PlayTestAudioTime <= 500 ? -1500 : -delay;
                        return;
                    }

                    AudioEngine.Track.Seek(MathHelper.Clamp((int)Screen.PlayTestAudioTime - delay, 0,
                        (int)AudioEngine.Track.Length));
                    PrepareTrackForPlayback();
                    Time = AudioEngine.Track.Time;
                    UseAudioTime = true;
                    return;
                }

                PrepareTrackForPlayback();
                ConfigureAudioStart();
            }
            catch (AudioEngineException e)
            {
                Logger.Error(e, LogType.Runtime);
            }

            // Set the base time to - the start delay.
            Time = -StartDelay * AudioEngine.Track.Rate;
        }

        /// <summary>
        ///     Updates the audio time of the track.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            // Don't bother updating if the game is paused or the user failed.
            if (Screen.IsPaused)
                return;

            var isTournanent = Screen is TournamentGameplayScreen;

            if (Screen.IsMultiplayerGame && !Screen.IsMultiplayerGameStarted && !isTournanent)
                return;

            // Count down using frame time, then start the prepared stream early enough to absorb device latency.
            if (!Screen.HasStarted)
            {
                if (Time < TimeToPlayAudio)
                {
                    Time += gameTime.ElapsedGameTime.TotalMilliseconds * AudioEngine.Track.Rate;

                    if (Time < TimeToPlayAudio)
                        return;
                }

                try
                {
                    AudioEngine.Track.Play();
                    Screen.HasStarted = true;
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    return;
                }

                return;
            }

            if (!UseAudioTime)
            {
                UpdateStartupClock(gameTime);
                return;
            }

            // Use frame time if the option is enabled.
            if (ConfigManager.SmoothAudioTimingGameplay.Value)
            {
                Time += gameTime.ElapsedGameTime.TotalMilliseconds * AudioEngine.Track.Rate;
                var checkTime = AudioEngine.Track.Time - PreviousTime;

                if (!AudioEngine.Track.IsPlaying)
                    return;

                // Time falls behind or goes too far ahead of the track
                const int threshold = 16;
                var timeOutOfThreshold = Time < AudioEngine.Track.Time || Time > AudioEngine.Track.Time + threshold * AudioEngine.Track.Rate;

                // More than a second passes without resyncing
                const int routineSyncTime = 1000;
                var needsRoutineSync = checkTime >= routineSyncTime || checkTime <= -routineSyncTime;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (!timeOutOfThreshold && !needsRoutineSync && !Screen.Failed && PreviousTime != 0)
                    return;

                Time = AudioEngine.Track.Time;
                PreviousTime = AudioEngine.Track.Time;
            }
            else
            {
                // If the audio track is playing, use that time.
                if (AudioEngine.Track.IsPlaying)
                    Time = Math.Max(Time, AudioEngine.Track.Time);
                // Otherwise use deltatime to calculate the proposed time.
                else
                    Time += gameTime.ElapsedGameTime.TotalMilliseconds * AudioEngine.Track.Rate;
            }
        }

        /// <summary>
        ///     Moves initial stream generation out of the playback-start frame.
        /// </summary>
        private static void PrepareTrackForPlayback()
        {
            var startedAt = Stopwatch.GetTimestamp();
            var prepared = AudioEngine.PrepareCurrentTrackForPlayback();
            var elapsed = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;

            if (!prepared)
                Logger.Warning("BASS could not prepare the gameplay track's playback buffer.", LogType.Runtime);
            else
                Logger.Debug($"Prepared gameplay audio in {elapsed:0.###} ms.", LogType.Runtime);
        }

        /// <summary>
        ///     Starts playback early enough for the first audio sample to reach the output at map time zero.
        /// </summary>
        private void ConfigureAudioStart()
        {
            var outputLatency = AudioEngine.Track is AudioTrack
                ? Math.Clamp((double)AudioManager.OutputLatency, 0, MaximumAudioPrestartMs)
                : 0;

            TimeToPlayAudio = -outputLatency * AudioEngine.Track.Rate;

            Logger.Debug(
                $"Gameplay audio will start at {TimeToPlayAudio:0.###} ms " +
                $"(device latency: {AudioManager.OutputLatency} ms).",
                LogType.Runtime);
        }

        /// <summary>
        ///     Keeps gameplay time monotonic while it converges with the newly started BASS playback clock.
        /// </summary>
        private void UpdateStartupClock(GameTime gameTime)
        {
            var rate = AudioEngine.Track.Rate;
            var elapsedTime = gameTime.ElapsedGameTime.TotalMilliseconds * rate;
            var proposedTime = Time + elapsedTime;
            var audioTime = AudioEngine.Track.Time;

            if (!AudioEngine.Track.IsPlaying || audioTime <= 0)
            {
                Time = proposedTime;
                return;
            }

            MaximumStartupClockError = Math.Max(MaximumStartupClockError, Math.Abs(audioTime - proposedTime));

            var correctionDuration = StartupCorrectionDurationMs * rate;
            var correction = (audioTime - proposedTime) * elapsedTime / correctionDuration;
            var maximumCorrection = elapsedTime * MaximumStartupCorrectionRate;
            correction = Math.Clamp(correction, -maximumCorrection, maximumCorrection);

            Time = Math.Max(Time, proposedTime + correction);

            var handoffThreshold = Math.Min(rate, elapsedTime);
            if (Math.Abs(audioTime - Time) > handoffThreshold)
                return;

            // A sub-frame difference is safe: by the next update the playback clock will have caught up.
            Time = Math.Max(Time, audioTime);
            PreviousTime = audioTime;
            UseAudioTime = true;

            Logger.Debug(
                $"Gameplay audio clock synchronized at {audioTime:0.###} ms " +
                $"(device latency: {AudioManager.OutputLatency} ms, " +
                $"maximum startup error: {MaximumStartupClockError:0.###} ms).",
                LogType.Runtime);
        }
    }
}
