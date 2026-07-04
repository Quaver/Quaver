/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
 */

using System;
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
        ///     We won't pre-start the audio further than this number of milliseconds.
        /// </summary>
        private const double AudioPrestartThresholdMs = 40;

        /// <summary>
        ///     The appropriate time to start playing the audio.
        ///     If SmoothAudioStart is enabled, this will be earlier than 0 due to audio start lag.
        /// </summary>
        private double TimeToPlayAudio { get; } = ConfigManager.SmoothAudioStart.Value
            ? -Math.Clamp(AudioEngine.MeasuredAudioStartDelay, 0, AudioPrestartThresholdMs) * AudioEngine.Track.Rate
            : 0;

        /// <summary>
        ///     The delay measured after it has been compensated by <see cref="AudioEngine.MeasuredAudioStartDelay"/>.
        ///     This is used to further slowly tweak the timing to match with the audio time.
        /// </summary>
        private double? MeasuredAudioStutter { get; set; }

        /// <summary>
        ///     Determines if we set Time to <see cref="AudioEngine.Track.Time"/>, or accumulate
        ///     using our game timer
        /// </summary>
        private bool UseAudioTime { get; set; } = !ConfigManager.SmoothAudioStart.Value;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        public GameplayAudioTiming(GameplayScreen screen)
        {
            Screen = screen;

            if (Screen.IsSongSelectPreview || Screen.UseExistingAudioTime)
                return;

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
                        Time = Screen.PlayTestAudioTime <= 500 ? -1500 : -delay;
                        return;
                    }

                    AudioEngine.Track.Seek(MathHelper.Clamp((int)Screen.PlayTestAudioTime - delay, 0,
                        (int)AudioEngine.Track.Length));
                    Time = AudioEngine.Track.Time;
                    return;
                }
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

            // If the audio hasn't begun yet, start counting down until the beginning of the map.
            // This is to give a delay before the audio starts.
            if (Time < TimeToPlayAudio)
            {
                Time += gameTime.ElapsedGameTime.TotalMilliseconds * AudioEngine.Track.Rate;
                return;
            }

            // Play the track if the game hasn't started yet.
            if (!Screen.HasStarted)
            {
                try
                {
                    Screen.HasStarted = true;
                    AudioEngine.Track.Play();
                }
                catch (Exception e)
                {
                    // ignored
                }
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
                const double correctionDurationMs = 1000;
                if (UseAudioTime || Time >= correctionDurationMs)
                {
                    // If the audio track is playing, use that time.
                    if (AudioEngine.Track.IsPlaying)
                        Time = AudioEngine.Track.Time;
                    // Otherwise use deltatime to calculate the proposed time.
                    else
                        Time += gameTime.ElapsedGameTime.TotalMilliseconds * AudioEngine.Track.Rate;
                }
                else
                {
                    var elapsedTime = gameTime.ElapsedGameTime.TotalMilliseconds * AudioEngine.Track.Rate;
                    var newTime = Time + elapsedTime;

                    // We will have incremented MeasuredAudioStutter to Time within correctionDurationMs
                    var correctedTime = newTime + (MeasuredAudioStutter ?? 0) *
                        gameTime.ElapsedGameTime.TotalMilliseconds / correctionDurationMs;
                    if (AudioEngine.Track.IsPlaying && AudioEngine.Track.Time != 0)
                    {
                        if (MeasuredAudioStutter == null)
                        {
                            const double correctionFactor = 0.5;
                            // This is the delay we have left
                            // We will use this as a correction to AudioEngine.MeasuredAudioStartDelay, so it will be
                            // better next time (hopefully)
                            MeasuredAudioStutter = AudioEngine.Track.Time - newTime;
                            var correctedDelay =
                                AudioEngine.MeasuredAudioStartDelay - MeasuredAudioStutter.Value * correctionFactor;

                            // We will correct it if we don't go out of threshold (this happens on 60fps)
                            // Or if the corrected delay is less (so if the original delay is higher than threshold,
                            //      it gets reduced)
                            if (correctedDelay <= AudioPrestartThresholdMs ||
                                correctedDelay < AudioEngine.MeasuredAudioStartDelay)
                            {
                                AudioEngine.MeasuredAudioStartDelay = Math.Clamp(correctedDelay, 0, AudioPrestartThresholdMs);
                                // Console.WriteLine($"Corrected delay to {AudioEngine.MeasuredAudioStartDelay}");
                            }

                            // Console.WriteLine($"Measured audio stutter = {MeasuredAudioStutter}");
                        }

                        // Correct timing. Don't go back in time.
                        if (correctedTime > Time)
                            newTime = correctedTime;

                        Time = newTime;

                        // The following 3 conditions will make us switch to audio time:
                        // 1. Audio Time and Time is already very close. It is unnoticeable to switch directly. 
                        // 2. Using the proposed corrected time will make the difference even larger.
                        //    This could happen if we somehow managed to skip the first check.
                        // 3. We are past the correction period. We shouldn't interfere with it anyhow.
                        if (Math.Abs(Time - AudioEngine.Track.Time) < 3
                            || Math.Abs(Time - AudioEngine.Track.Time) < Math.Abs(correctedTime - AudioEngine.Track.Time)
                            || Time >= correctionDurationMs)
                        {
                            UseAudioTime = true;
                            // Console.WriteLine($"Compensation complete at time={Time} audioTime={AudioEngine.Track.Time}");
                        }
                    }
                    else
                    {
                        Time = newTime;
                    }
                    // Console.WriteLine($"Time: {Time}\t\t Audio Time: {AudioEngine.Track.Time}\t\t Diff: {AudioEngine.Track.Time - Time}");
                }
            }
        }
    }
}