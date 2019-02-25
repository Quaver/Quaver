/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.Shared.Scheduling
{
    public class TaskCancelledEventArgs<T> : EventArgs
    {
        /// <summary>
        ///     Input given to task before cancelled
        /// </summary>
        public T Input { get; }

        public TaskCancelledEventArgs (T input) => Input = input
    }
}
