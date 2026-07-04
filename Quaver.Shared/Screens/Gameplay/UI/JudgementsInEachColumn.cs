/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;

namespace Quaver.Shared.Screens.Gameplay.UI
{
    /// <summary>
    ///     Which judgements get their own hit burst displayed in their column
    ///     instead of the normal hit burst.
    /// </summary>
    [Flags]
    public enum JudgementsInEachColumn
    {
        False = 0,
        Marv = 1 << 0,
        Perf = 1 << 1,
        Great = 1 << 2,
        Good = 1 << 3,
        Okay = 1 << 4,
        Miss = 1 << 5,
        Default = False,
        NoMarv = Perf | Great | Good | Okay | Miss,
        All = Marv | NoMarv
    }
}
