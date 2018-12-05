/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

namespace Quaver.Shared.Skinning
{
    public static class SkinManager
    {
        /// <summary>
        ///     The currently selected skin
        /// </summary>
        public static SkinStore Skin { get; private set; }

        /// <summary>
        ///     Loads the currently selected skin
        /// </summary>
        public static void Load() => Skin = new SkinStore();
    }
}
