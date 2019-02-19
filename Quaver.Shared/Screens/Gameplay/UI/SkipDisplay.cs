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
using Quaver.Shared.Audio;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI
{
    public class SkipDisplay : AnimatableSprite
    {
        /// <summary>
        ///     Reference to the gameplay screen.
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="frames"></param>
        internal SkipDisplay(GameplayScreen screen, List<Texture2D> frames) : base(frames)
        {
            Screen = screen;
            Size = new ScalableVector2(230, 56);
            Y = 30;
            Alignment = Alignment.TopCenter;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            if (Screen.EligibleToSkip)
                StartLoop(Direction.Forward, (int)(30 * AudioEngine.Track.Rate));
            else
                StopLoop();

            var targetAlpha = Screen.EligibleToSkip ? 1 : 0;
            Alpha = MathHelper.Lerp(Alpha, targetAlpha, (float) Math.Min(dt / (120 / AudioEngine.Track.Rate), 1));

            base.Update(gameTime);
        }
    }
}
