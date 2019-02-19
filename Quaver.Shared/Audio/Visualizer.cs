/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using ManagedBass;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Audio
{
    public class Visualizer : Sprite
    {
        /// <summary>
        ///     Holds the FFT sample data
        /// </summary>
        private float[] SampleData { get; } = new float[512];

        private float[] FrequencyBands { get; set; } = new float[8];

        public List<Sprite> Bars { get; }

        public Visualizer()
        {
            Alignment = Alignment.MidCenter;
            Size = new ScalableVector2(WindowManager.Width, 75);
            Tint = Color.Black;
            Alpha = 0.45f;
            Y = -10;

            Bars = new List<Sprite>();

            for (var i = 0; i < 8; i++)
            {
                Bars.Add(new Sprite()
                {
                    Parent = this,
                    Width = 20,
                    X = i * 25,
                    Tint = Color.Red,
                    Alignment = Alignment.MidCenter
                });
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (AudioEngine.Track != null && !AudioEngine.Track.IsDisposed && AudioEngine.Track.IsPlaying)
            {
                Bass.ChannelGetData(AudioEngine.Track.Stream, SampleData, 512);

                var count = 0;

                for (var i = 0; i < 8; i++)
                {
                    var average = 0f;
                    var sampleCount = (int) Math.Pow(2, i) * 2;

                    if (i == 7)
                    {
                        sampleCount += 2;
                    }

                    for (var j = 0; j < sampleCount; j++)
                    {
                        average += SampleData[count] * (count + 1);
                        count++;
                    }

                    average /= count;
                    FrequencyBands[i] = average * 10;

                    Bars[i].Height = FrequencyBands[i];
                    // Console.WriteLine(Bars[i].Height);
                }
            }

            base.Update(gameTime);
        }
    }
}
