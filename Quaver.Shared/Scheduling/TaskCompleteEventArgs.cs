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
    public class TaskCompleteEventArgs<T, TResult> : EventArgs
    {
        /// <summary>
        ///     Input given to task
        /// </summary>
        public T Input { get; }

        /// <summary>
        ///     Result of the task
        /// </summary>
        public TResult Result { get; }

        public TaskCompleteEventArgs(T input, TResult result)
        {
            Input = Input;
            Result = result;
        }
    }
}
