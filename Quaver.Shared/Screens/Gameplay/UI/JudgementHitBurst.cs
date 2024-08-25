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
using MonoGame.Extended.Timers;
using Quaver.API.Enums;
using Quaver.Shared.Audio;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
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
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <summary>
        ///     If we are currently animating the hit burst with only one frame.
        /// </summary>
        public bool IsAnimatingWithOneFrame { get; private set; }

        /// <summary>
        ///     Timer for bumping the burst if <see cref="IsAnimatingWithOneFrame"/>
        /// </summary>
        private readonly CountdownTimer bumpTimer;

        /// <summary>
        ///     Time to bump
        /// </summary>
        private readonly TimeSpan bumpTime;

        /// <summary>
        ///     Start Y of bumping
        /// </summary>
        private readonly float bumpY;

        /// <summary>
        ///     The original size of the hit burst.
        /// </summary>
        private Vector2 OriginalSize { get; }

        /// <summary>
        ///     The original Y position of the hit burst.
        /// </summary>
        public float OriginalPosY { get; set; }

        private SkinKeys Skin => SkinManager.Skin.Keys[Screen.Map.Mode];

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="frames"></param>
        /// <param name="size"></param>
        /// <param name="posY"></param>
        public JudgementHitBurst(GameplayScreen screen, List<Texture2D> frames, Vector2 size, float posY) : base(frames)
        {
            Screen = screen;
            OriginalPosY = posY;
            OriginalSize = size;
            Size = new ScalableVector2(OriginalSize.X, OriginalSize.Y);
            Y = OriginalPosY;
            Visible = false;

            // Whenever the judgement is finished looping, then we'll make it invisible.
            FinishedLooping += (o, e) => Visible = false;

            bumpTime = TimeSpan.FromMilliseconds(Skin.JudgementHitBurstBumpTime);
            bumpTimer = new CountdownTimer(bumpTime);
            bumpTimer.TimeRemainingChanged += LerpY;
            bumpTimer.Stopped += LerpY;
            bumpY = OriginalPosY + Skin.JudgementHitBurstBumpY;
        }

        private void LerpY(object sender, EventArgs e)
        {
            var t = 1 - bumpTimer.TimeRemaining / bumpTime;
            Y = EasingFunctions.EaseOutExpo(bumpY, OriginalPosY, (float)t);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PerformOneFrameAnimation(gameTime);

            base.Update(gameTime);
            bumpTimer.Update(gameTime);
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

            Alpha = 1;

            if (Frames.Count != 1)
            {
                ChangeTo(0);
                StartLoop(Direction.Forward, Skin.JudgementHitBurstFps, 1);
                IsAnimatingWithOneFrame = false;
            }
            else
            {
                bumpTimer.Restart();
                IsAnimatingWithOneFrame = true;
            }

            var firstFrame = Frames[0];
            var scale = SkinManager.Skin.Keys[Screen.Map.Mode].JudgementHitBurstScale / firstFrame.Height;

            var (x, y) = new Vector2(firstFrame.Width, firstFrame.Height) * scale;
            Size = new ScalableVector2(x, y);
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
            if (bumpTimer.State == TimerState.Completed)
            {
                Alpha = MathHelper.Lerp(Alpha, 0, (float) Math.Min(dt / 240, 1));

                if (Alpha <= 0)
                    IsAnimatingWithOneFrame = false;
            }
        }
    }
}
