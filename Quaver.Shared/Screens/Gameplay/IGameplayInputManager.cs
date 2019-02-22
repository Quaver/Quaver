/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

namespace Quaver.Shared.Screens.Gameplay
{
    public interface IGameplayInputManager
    {
        /// <summary>
        ///     Handles all of the input for the entire ruleset.
        /// </summary>
        /// <param name="dt"></param>
        void HandleInput(double dt);
    }
}
