/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.Shared.Database.Maps
{
    public class ImportingMapsetEventArgs : EventArgs
    {
        /// <summary>
        ///     The queue that the was imported along with the mapset.
        /// </summary>
        public List<string> Queue { get; }

        /// <summary>
        ///     The mapset's filename.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        ///     The mapset's index in the Import Queue.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="success"></param>
        /// <param name="name"></param>
        /// <param name="index"></param>
        public ImportingMapsetEventArgs(List<string> queue, string name, int index)
        {
            Queue = queue;
            FileName = name;
            Index = index;
        }
    }
}
