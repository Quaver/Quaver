/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Quaver.API.Enums;

namespace Quaver.Shared.Modifiers
{
    public class ModsChangedEventArgs : EventArgs
    {
        /// <summary>
        /// </summary>
        public ModChangeType Type { get; }

        /// <summary>
        ///     The newly activated mods/
        /// </summary>
        public ModIdentifier Mods { get; }

        /// <summary>
        ///     The mods that were changed
        /// </summary>
        public ModIdentifier ChangedMods { get; }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="mods"></param>
        /// <param name="changedMods"></param>
        public ModsChangedEventArgs(ModChangeType type, ModIdentifier mods, ModIdentifier changedMods)
        {
            Type = type;
            Mods = mods;
            ChangedMods = changedMods;
        }
    }
}
