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
        ///     Used to determine when to sync Time when UseFrameTime is on.
        /// </summary>
        public double OldTime = 0;

        /// <summary>
        ///     The time in the audio/play.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        public GameplayAudioTiming(GameplayScreen screen)
        {
            Screen = screen;

            if (Screen.IsSongSelectPreview)
                return;

            try
            {
                if (Screen.IsCalibratingOffset)
                    AudioEngine.Track = new AudioTrack(GameBase.Game.Resources.Get($"Quaver.Resources/Maps/Offset/offset.mp3"));
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
                        Time = -delay;
                        return;
                    }

                    AudioEngine.Track.Seek(MathHelper.Clamp((int) Screen.PlayTestAudioTime - delay, 0, (int) AudioEngine.Track.Length));
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
            if (Time < 0)
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
                    AudioEngine.Track?.Play();
                }
                catch (Exception e)
                {
                    // ignored
                }
            }

            // Use frame time if the option is enabled.
            if (ConfigManager.UseFrameTime.Value)
            {
                Time += gameTime.ElapsedGameTime.TotalMilliseconds * AudioEngine.Track.Rate;
                var CheckTime = AudioEngine.Track.Time - OldTime;

                // If Time falls behind the audio track or more than a second passes without syncing, resync. If Failed, use audio track time for slowdown animation.
                if (AudioEngine.Track.IsPlaying && (Time < AudioEngine.Track.Time || CheckTime >= 1000 || CheckTime <= -1000 || OldTime == 0 || Screen.Failed))
                {
                    Time = AudioEngine.Track.Time;
                    OldTime = AudioEngine.Track.Time;
                }

            }
            // Otherwise use AudioEngine time.
            else
            {
                // If the audio track is playing, use that time.
                if (AudioEngine.Track.IsPlaying)
                    Time = AudioEngine.Track.Time;

                // Otherwise use deltatime to calculate the proposed time.
                else
                    Time += gameTime.ElapsedGameTime.TotalMilliseconds * AudioEngine.Track.Rate;
            }
        }
    }
}
