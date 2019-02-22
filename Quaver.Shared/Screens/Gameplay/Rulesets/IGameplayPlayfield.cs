/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Gameplay.Rulesets
{
    public interface IGameplayPlayfield : IGameScreenComponent
    {
        /// <summary>
        ///     Container that has the entire playfield in it.
        /// </summary>
        Container Container { get; set; }

        /// <summary>
        ///     Handles what happens to the playfield on failure.
        /// </summary>
        /// <param name="gameTime"></param>
        void HandleFailure(GameTime gameTime);
    }
}
