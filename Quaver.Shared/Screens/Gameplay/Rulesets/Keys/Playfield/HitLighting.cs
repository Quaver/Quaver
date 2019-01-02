/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield
{
    public class HitLighting : AnimatableSprite
    {
        /// <summary>
        ///     If we're curerntly holding a long note.
        ///     It'll loop through the animation until we aren't anymore.
        /// </summary>
        private bool IsHoldingLongNote { get; set; }

        /// <summary>
        ///     If we're currently performing a one frame animation.
        /// </summary>
        private bool PerformingOneFrameAnimation { get; set; }

        /// <summary>
        ///     Dictates if we're currently decreasing the alpha in the one frame LN
        ///     hold animation.
        /// </summary>
        private bool DecreasingAlphaInAnimation { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public HitLighting() : base(SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].HitLighting) => FinishedLooping += OnLoopCompletion;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Performs a one animation frame if possible.
            if (PerformingOneFrameAnimation)
                PerformOneFrameAnimation(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        ///     When hitting an object, it'll loop through once.
        /// </summary>
        public void PerformHitAnimation(bool isLongNote)
        {
            var skin = SkinManager.Skin.Keys[MapManager.Selected.Value.Mode];
            IsHoldingLongNote = isLongNote;

            // First begin by replacing the frames
            ReplaceFrames(IsHoldingLongNote ? skin.HoldLighting : skin.HitLighting);

            // Go to the first frame and reset each of the properties
            ChangeTo(0);
            Visible = true;
            Alpha = 1;

            // If we are performing a one frame animation however, we don't want to handle it
            // through standard looping, but rather through our own rolled out animation.
            PerformingOneFrameAnimation = Frames.Count == 1;
            if (PerformingOneFrameAnimation)
                return;

            // Standard looping animations.
            if (!IsHoldingLongNote)
                StartLoop(Direction.Forward, (int)(skin.HitLightingFps * AudioEngine.Track.Rate), 1);
            else
                StartLoop(Direction.Forward, (int)(skin.HoldLightingFps * AudioEngine.Track.Rate));
        }

        /// <summary>
        ///     Stops holding (looping forever). Used when the user isn't holding the LN anymore.
        /// </summary>
        public void StopHolding()
        {
            StopLoop();
            Visible = false;
            IsHoldingLongNote = false;
            PerformingOneFrameAnimation = false;
        }

        /// <summary>
        ///     When the animation loop is completed, we'll dictate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoopCompletion(object sender, EventArgs e)
        {
            // If the loop is done and this isn't a long note, then we'll want to make it invisible.
            if (IsHoldingLongNote)
                return;

            Visible = false;
            PerformingOneFrameAnimation = false;
        }

        /// <summary>
        ///     Performs all one frame animations for both normal notes and LN holding.
        /// </summary>
        /// <param name="gameTime"></param>
        private void PerformOneFrameAnimation(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Animation for normal HitObjects
            if (!IsHoldingLongNote)
            {
                Alpha -= AlphaChangePerFrame(dt);

                if (Alpha <= 0)
                    FinishedLooping?.Invoke(this, null);
            }
            // Animation for LN HitObjects.
            // Pulsate the alpha of it.
            else
            {
                if (Alpha >= 1)
                    DecreasingAlphaInAnimation = true;

                if (DecreasingAlphaInAnimation)
                {
                    Alpha -= AlphaChangePerFrame(dt);

                    if (Alpha <= 0)
                        DecreasingAlphaInAnimation = false;
                }
                else
                {
                    Alpha += AlphaChangePerFrame(dt);
                }
            }
        }

        /// <summary>
        ///     The amount of alpha change per frame when doing one frame animations.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static float AlphaChangePerFrame(double dt) => (float)(dt / (60 * AudioEngine.Track.Rate));
    }
}
