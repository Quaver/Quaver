using System;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;

namespace Quaver.States.Gameplay.GameModes.Keys.Playfield
{
    internal class ColumnLighting
    {
        /// <summary>
        ///     The actual column lighting sprite.
        /// </summary>
        private QuaverSprite Sprite { get; }

        /// <summary>
        ///     If the column lighting is currently active.
        /// </summary>
        internal bool Active { private get; set; }

        /// <summary>
        ///     The animation for this column lighting
        /// </summary>
        internal float AnimationValue { private get; set; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="sprite"></param>
        internal ColumnLighting(QuaverSprite sprite)
        {
            Sprite = sprite;
        }

        /// <summary>
        ///     Performs 
        /// </summary>
        /// <param name="dt"></param>
        internal void PerformAnimation(double dt)
        {
            // Update the animation value based on if it's active or not.
            if (Active)
                AnimationValue = GraphicsHelper.Tween(1, AnimationValue, Math.Min(dt / 2, 1));
            else
                AnimationValue = GraphicsHelper.Tween(0, AnimationValue, Math.Min(dt / 60, 1));
                
            // Update the alpha of the sprite.
            Sprite.Alpha = AnimationValue;
        }
    }
}