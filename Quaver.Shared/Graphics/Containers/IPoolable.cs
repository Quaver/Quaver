using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics.Containers
{
    public interface IPoolable<T>
    {
        /// <summary>
        ///     Updates the content of the object with the
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        void UpdateContent(T item, int index);
    }
}