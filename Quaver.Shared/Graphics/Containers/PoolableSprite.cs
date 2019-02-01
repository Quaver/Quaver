using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics.Containers
{
    public abstract class PoolableSprite<T> : Sprite, IPoolable<T>
    {
        /// <summary>
        ///     The height of the object.
        /// </summary>
        public abstract int HEIGHT { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public abstract void UpdateContent(T item, int index);
    }
}