/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics.Containers
{
    public abstract class PoolableSprite<T> : Sprite, IPoolable<T>
    {
        /// <summary>
        /// </summary>
        public PoolableScrollContainer<T> Container { get; }

        /// <summary>
        ///     The item that this sprite represents
        /// </summary>
        public T Item { get; protected set; }

        /// <summary>
        ///     The index that this sprite is in the pool
        /// </summary>
        public int Index { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public PoolableSprite(PoolableScrollContainer<T> container, T item, int index)
        {
            Container = container;
            Item = item;
            Index = index;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            if (Rectangle.Intersect(ScreenRectangle, Container.ScreenRectangle).IsEmpty)
                return;

            base.Draw(gameTime);
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
