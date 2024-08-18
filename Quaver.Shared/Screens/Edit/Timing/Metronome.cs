/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.API.Maps;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Skinning;
using TagLib.Matroska;
using Wobble;
using Wobble.Audio.Samples;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using IDrawable = Wobble.Graphics.IDrawable;

namespace Quaver.Shared.Screens.Editor.Timing
{
    public class Metronome : IUpdate, IDisposable
    {
        /// <summary>
        /// </summary>
        private Qua Qua { get; }

        /// <summary>
        /// </summary>
        private IAudioTrack Track { get; }

        /// <summary>
        /// </summary>
        private BindableInt GlobalAudioOffset { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> PlayHalfBeats { get; }

        /// <summary>
        /// </summary>
        private AudioSample BeatTickSample { get; }

        /// <summary>
        /// </summary>
        private AudioSample MeasureTickSample { get; }

        /// <summary>
        ///     The beat that was on in the last frame.
        /// </summary>
        private int LastBeat { get; set; }

        /// <summary>
        ///     The current beat we are on in this frame.
        /// </summary>
        public int CurrentBeat { get; set; }

        /// <summary>
        ///     The amount of total beats in the current frame.
        /// </summary>
        private int CurrentTotalBeats { get; set; }

        /// <summary>
        ///     The amount of total beats in the last frame
        /// </summary>
        private int LastTotalBeats { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="qua"></param>
        /// <param name="track"></param>
        /// <param name="audioOffset"></param>
        /// <param name="playHalfBeats"></param>
        public Metronome(Qua qua, IAudioTrack track = null, BindableInt audioOffset = null, Bindable<bool> playHalfBeats = null)
        {
            Qua = qua;
            Track = track ?? AudioEngine.Track;
            GlobalAudioOffset = audioOffset ?? ConfigManager.GlobalAudioOffset;
            PlayHalfBeats = playHalfBeats ?? ConfigManager.EditorMetronomePlayHalfBeats;

            BeatTickSample = new AudioSample(GameBase.Game.Resources.Get($"Quaver.Resources/SFX/Editor/metronome-beat.wav"));
            MeasureTickSample = new AudioSample(GameBase.Game.Resources.Get($"Quaver.Resources/SFX/Editor/metronome-measure.wav"));
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (!Track.IsPlaying || Qua.TimingPoints.Count == 0)
                return;

            var time = Track.Time + GlobalAudioOffset.Value;

            var point = Qua.GetTimingPointAt(time);

            if (time < point.StartTime)
            {
                LastBeat = -1;
                return;
            }

            // Get the total amount of beats that'll be played for the timing point.
            // This can depend on if the user wants 8 beats or 4.
            var totalBeats = (time - point.StartTime) / (point.MillisecondsPerBeat / (PlayHalfBeats.Value ? 2 : 1));

            CurrentTotalBeats = (int)Math.Floor(totalBeats);

            var signature = point.Signature != 0 ? Math.Abs((int)point.Signature) : 4;
            CurrentBeat = (int)totalBeats % signature;

            // Play samples
            if (CurrentTotalBeats == 0 && LastTotalBeats < 0 || CurrentBeat != LastBeat)
            {
                if (CurrentBeat == 0)
                    MeasureTickSample.CreateChannel().Play();
                else
                    BeatTickSample.CreateChannel().Play();
            }

            // Keep track of the last beat & last total beats in the current frame, so we know when to play
            // the next sample.
            LastBeat = CurrentBeat;
            LastTotalBeats = CurrentTotalBeats;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose()
        {
            BeatTickSample?.Dispose();
            MeasureTickSample?.Dispose();
        }
    }
}