/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

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
