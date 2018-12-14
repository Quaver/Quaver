/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Audio;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Edit.Rulesets.Keys.Playfield
{
    public class EditorSnapLinesKeys : Container
    {
        /// <summary>
        ///     Reference to the editor screen itself.
        /// </summary>
        private EditorScreen Screen { get; }

        /// <summary>
        ///     The 
        /// </summary>
        private EditorPlayfieldKeys Playfield { get; }

        /// <summary>
        ///     The buffer of snap lines.
        /// </summary>
        private List<Sprite> LineBuffer { get; }

        /// <summary>
        ///     The current index we're in on the object pool.
        /// </summary>
        private int PoolIndex { get; set; }

        /// <summary>
        ///     The amount of lines that'll be pooled.
        /// </summary>
        private int PoolSize { get; } = byte.MaxValue;

        /// <summary>
        ///     The last captured audio time in the previous frame.
        /// </summary>
        private double LastAudioTime { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="playfield"></param>
        public EditorSnapLinesKeys(EditorScreen screen, EditorPlayfieldKeys playfield)
        {
            Screen = screen;
            Playfield = playfield;
            Parent = Playfield.BackgroundContainer;
            LineBuffer = new List<Sprite>();

            for (var i = 0; i < PoolSize; i++)
            {
                var line = new Sprite
                {
                    Parent = this,
                    Size = new ScalableVector2(Playfield.Width, 1),
                    Alignment = Alignment.TopLeft,
                    Y = -1
                };

                LineBuffer.Add(line);
            }

            Screen.BeatSnap.ValueChanged += OnSnapChanged;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Moves the position of the lines.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var tp = Screen.Map.GetTimingPointAt(AudioEngine.Track.Time);
            var timePerSnap = tp.MillisecondsPerBeat / Screen.BeatSnap.Value;

            // User seeked backwards in the audio, so we need to pool backwards.
            if (AudioEngine.Track.Time < LastAudioTime)
            {
                // Get the amount of time that was skipped back
                var timeSeekedBack = AudioEngine.Track.Time - LastAudioTime;

                // Get the amount of beats that should be pushed back.
                var beats = (int)Math.Abs(timeSeekedBack / timePerSnap) + 1;

                for (var i = 0; i < beats; i++)
                {
                    var lastIndex = PoolSize + PoolIndex - 1;

                    // Take the line at the last index and put it at the first
                    var line = LineBuffer[lastIndex];

                    LineBuffer.RemoveAt(lastIndex);

                    // Decrease the pool index
                    PoolIndex--;

                    // Put the line at the new pool index.
                    LineBuffer[PoolIndex] = line;
                }
            }

            // Set the positions of each line.
            for (var i = PoolIndex; i < PoolSize + PoolIndex; i++)
            {
                var targetTime = tp.StartTime + timePerSnap * i;

                var line = LineBuffer[i];

                line.Height = i % Screen.BeatSnap.Value == 0 ? 5 : 1;
                var newPos = EditorScrollContainerKeys.GetPosFromOffset(Playfield.HitPositionY, Playfield.ScrollSpeed, targetTime, line.Height);

                // Set the new position if it is indeed on-screen.
                line.Y = (newPos > 0) ? newPos : -1;

                // Pooling starts when a line has gone off-screen, this is when the audio is going forward,
                // so we need to pool forward.
                if (!(LineBuffer[PoolIndex].Y > WindowManager.Height))
                    continue;

                // Add a new line.
                LineBuffer.Add(line);

                // Make the line in the old position null.
                LineBuffer[PoolIndex] = null;

                // Increase the index of the object pooling.
                PoolIndex++;
            }

            LastAudioTime = AudioEngine.Track.Time;
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            Screen.BeatSnap.ValueChanged -= OnSnapChanged;

            base.Destroy();
        }

        /// <summary>
        ///     When the snap changes backwards, we want to remove any extra lines.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnSnapChanged(object sender, BindableValueChangedEventArgs<int> e)
        {
            if (e.Value > e.OldValue)
                return;

            Console.WriteLine("Lines need to be pooled back.");
        }
    }
}
