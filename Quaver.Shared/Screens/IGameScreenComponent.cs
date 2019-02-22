/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;

namespace Quaver.Shared.Screens
{
    public interface IGameScreenComponent
    {
        /// <summary>
        ///     Update, part of the game loop
        /// </summary>
        /// <param name="gameTime"></param>
        void Update(GameTime gameTime);

        /// <summary>
        ///     Draws the game component
        /// </summary>
        void Draw(GameTime gameTime);

        /// <summary>
        ///     Unloads/Frees memory from this component
        /// </summary>
        void Destroy();
    }
}
