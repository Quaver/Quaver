/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects
{
    public abstract class GameplayHitObject
    {
        /// <summary>
        ///     The info of this particular HitObject from the map file.
        /// </summary>
        public HitObjectInfo Info { get; set; }

        /// <summary>
        ///     Destroys the HitObject
        /// </summary>
        public abstract void Destroy();
    }
}
