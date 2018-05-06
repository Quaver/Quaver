using Quaver.Graphics.Sprites;

namespace Quaver.States.Gameplay.GameModes.Keys.Playfield
{
    internal class ColumnLighting
    {
        /// <summary>
        ///     The actual column lighting sprite.
        /// </summary>
        internal QuaverSprite Sprite { get; }

        /// <summary>
        ///     If the column lighting is currently active.
        /// </summary>
        internal bool Active { get;  }

        /// <summary>
        ///     The animation for this column lighting
        /// </summary>
        internal float Animation { get; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="sprite"></param>
        internal ColumnLighting(QuaverSprite sprite)
        {
            Sprite = sprite;
        }
    }
}