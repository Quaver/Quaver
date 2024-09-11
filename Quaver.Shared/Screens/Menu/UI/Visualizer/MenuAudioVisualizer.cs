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
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Menu.UI.Visualizer
{
    public class MenuAudioVisualizer : Sprite
    {
        /// <summary>
        ///     The number of bars in the visualizer.
        /// </summary>
        public List<Sprite> Bars { get; }

        /// <summary>
        ///     The max height of the bars.
        /// </summary>
        public int MaxBarHeight { get; }

        /// <summary>
        ///     Timer used to record time passed since last interpolation
        /// </summary>
        private TimeSpan interpolationTimer = TimeSpan.Zero;

        /// <summary>
        ///     Minimum time that needs to pass before the bars are interpolated again
        /// </summary>
        private readonly TimeSpan barInterpolateInterval = TimeSpan.FromMilliseconds(50);

        private readonly float[]  spectrumData = new float[2048];

        /// <inheritdoc />
        ///   <summary>
        ///   </summary>
        ///   <param name="width"></param>
        ///   <param name="maxHeight"></param>
        ///   <param name="numBars"></param>
        ///  <param name="barWidth"></param>
        public MenuAudioVisualizer(int width, int maxHeight, int numBars, int barWidth, int spacing = 5)
        {
            MaxBarHeight = maxHeight;

            Size = new ScalableVector2(width, maxHeight);
            Alpha = 0f;

            Bars = new List<Sprite>();

            for (var i = 0; i < numBars; i++)
            {
                var bar = new Sprite()
                {
                    Parent = this,
                    Alignment = Alignment.BotLeft,
                    Tint = Colors.MainAccentInactive,
                    Width = barWidth,
                    X = barWidth * i + i * spacing,
                    Alpha = 0.20f
                };

                Bars.Add(bar);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (ConfigManager.DisplayMenuAudioVisualizer != null && ConfigManager.DisplayMenuAudioVisualizer.Value)
            {
                interpolationTimer += gameTime.ElapsedGameTime;
                // ReSharper disable once InconsistentlySynchronizedField
                if (interpolationTimer >= barInterpolateInterval)
                {
                    interpolationTimer = TimeSpan.Zero;
                    InterpolateBars();
                }
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            if (ConfigManager.DisplayMenuAudioVisualizer != null && ConfigManager.DisplayMenuAudioVisualizer.Value)
                base.Draw(gameTime);
        }

        /// <summary>
        ///     Changes the height of the bars.
        /// </summary>
        private void InterpolateBars()
        {
            if (AudioEngine.Track == null || AudioEngine.Track.IsDisposed)
                return;

            if (AudioEngine.Track.IsPlaying)
                _ = Bass.ChannelGetData(AudioEngine.Track.Stream, spectrumData, (int)DataFlags.FFT2048);
            else
                Array.Clear(spectrumData);

            for (var i = 0; i < Bars.Count; i++)
            {
                var bar = Bars[i];

                var targetHeight = spectrumData[i] * MaxBarHeight;

                bar.Visible = targetHeight > 1f;

                if (!bar.Visible)
                    continue;

                // Lock the Animations to prevent any current updates.
                lock (bar.Animations)
                {
                    bar.Animations.Clear();
                    bar.Animations.Add(new Animation(AnimationProperty.Height, Easing.Linear,
                        bar.Height, targetHeight, (float)barInterpolateInterval.TotalMilliseconds));
                }
            }
        }
    }
}
