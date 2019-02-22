/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Shared.Audio;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI
{
    /// <inheritdoc />
    /// <summary>
    ///     Animatable sprite used when a user hits an object.
    ///     It is capable of switching  judgements and performing the loop animation once.
    /// </summary>
    public class JudgementHitBurst : AnimatableSprite
    {
        /// <summary>
        ///     If we are currently animating the hit burst with only one frame.
        /// </summary>
        public bool IsAnimatingWithOneFrame { get; private set; }

        /// <summary>
        ///     The original size of the hit burst.
        /// </summary>
        private Vector2 OriginalSize { get; }

        /// <summary>
        ///     The original Y position of the hit burst.
        /// </summary>
        private float OriginalPosY { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="size"></param>
        /// <param name="posY"></param>
        public JudgementHitBurst(List<Texture2D> frames, Vector2 size, float posY) : base(frames)
        {
            OriginalPosY = posY;
            OriginalSize = size;
            Size = new ScalableVector2(OriginalSize.X, OriginalSize.Y);
            Y = OriginalPosY;
            Visible = false;

            // Whenever the judgement is finished looping, then we'll make it invisible.
            FinishedLooping += (o, e) => Visible = false;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PerformOneFrameAnimation(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Replaces the animation frames with ones pertaining to the given judgement.
        /// </summary>
        /// <param name="j"></param>
        public void ChangeJudgementFrames(Judgement j)
        {
            if (IsLooping)
                StopLoop();

            ReplaceFrames(SkinManager.Skin.Judgements[j]);
        }

        /// <summary>
        ///     Switches to the correct judgement and performs a loop for it.
        /// </summary>
        /// <param name="j"></param>
        public void PerformJudgementAnimation(Judgement j)
        {
            ChangeJudgementFrames(j);
            Visible = true;

            if (Frames.Count != 1)
                StartLoop(Direction.Forward, (int)(30 * AudioEngine.Track.Rate), 1);
            else
            {
                // Set the position to slightly above, so we can tween it back down in the animation.
                Y = OriginalPosY - 5;
                Alpha = 1;
                IsAnimatingWithOneFrame = true;
            }
        }

        /// <summary>
        ///     If there is only 1 frame in the list, then we'll roll out our own animations here.
        /// </summary>
        /// <param name="gameTime"></param>
        private void PerformOneFrameAnimation(GameTime gameTime)
        {
            if (!IsAnimatingWithOneFrame)
                return;

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Tween the position if need be
            if (Math.Abs(Y - OriginalPosY) > 0.01)
                Y = MathHelper.Lerp(Y, OriginalPosY, (float) Math.Min(dt / (30 / AudioEngine.Track.Rate), 1));
            // If we've already tweened it, then we can begin to fade it out.
            else
            {
                Alpha = MathHelper.Lerp(Alpha, 0, (float) Math.Min(dt / ( 240 / AudioEngine.Track.Rate ), 1));

                if (Alpha <= 0)
                    IsAnimatingWithOneFrame = false;
            }
        }
    }
}
