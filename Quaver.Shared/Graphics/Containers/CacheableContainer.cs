using Microsoft.Xna.Framework;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Containers
{
    public abstract class CacheableContainer : Container
    {
        /// <summary>
        ///     If true, it will call <see cref="Cache"/>
        ///     at the beginning of <see cref="Draw"/>
        /// </summary>
        protected bool NeedsToCache { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            if (NeedsToCache)
            {
                Cache();
                NeedsToCache = false;
            }

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Performs any caching logic
        /// </summary>
        public abstract void Cache();
    }
}