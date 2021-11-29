/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Wobble.Bindables;
using Wobble.Input;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Input
{
    public class InputBindingKeys
    {
        /// <summary>
        ///     The key that this maps to.
        /// </summary>
        public Bindable<GenericKey> Key { get; }

        /// <summary>
        ///     If the key is currently pressed.
        /// </summary>
        public bool Pressed { get; set; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="key"></param>
        public InputBindingKeys(Bindable<GenericKey> key) => Key = key;
    }
}
