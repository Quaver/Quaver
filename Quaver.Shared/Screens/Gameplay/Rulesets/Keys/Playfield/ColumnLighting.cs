using System;
using Microsoft.Xna.Framework;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield
{
    public class ColumnLighting : Sprite
    {
        /// <summary>
        ///     Determines if the column lighting is currently active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        ///     The scale it takes to lerp during active animation time
        /// </summary>
        public int AnimationScale => Active ? 2 : 60;

        /// <summary>
        ///     The target alpha in which the column lighting will be.
        /// </summary>
        public int TargetAlpha => Active ? 1 : 0;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            Alpha = MathHelper.Lerp(Alpha, TargetAlpha, (float) Math.Min(dt / AnimationScale, 1));

            base.Update(gameTime);
        }
    }
}
