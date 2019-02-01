using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics.Containers
{
    public abstract class PoolableSprite<T> : Sprite, IPoolable<T>
    {
        /// <summary>
        ///     The item that this sprite represents
        /// </summary>
        public T Item { get; protected set; }

        /// <summary>
        ///     The index that this sprite is in the pool
        /// </summary>
        public int Index { get; protected set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public PoolableSprite(T item, int index)
        {
            Item = item;
            Index = index;
        }

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