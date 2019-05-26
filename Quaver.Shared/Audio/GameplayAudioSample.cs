/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Wobble.Audio.Samples;

namespace Quaver.Shared.Audio
{
    /// <summary>
    ///     An audio sample such as a sound effect or a keysound.
    /// </summary>
    public class GameplayAudioSample : IDisposable
    {
        /// <summary>
        ///     The underlying AudioSample.
        /// </summary>
        public AudioSample Sample { get; }

        /// <summary>
        ///     Whether the audio sample is unaffected by rate.
        /// </summary>
        public bool UnaffectedByRate { get; }

        public GameplayAudioSample(AudioSample sample, bool unaffectedByRate)
        {
            Sample = sample;
            UnaffectedByRate = unaffectedByRate;
        }

        public void Dispose() => Sample?.Dispose();
    }
}