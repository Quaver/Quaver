/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;

namespace Quaver.Shared.Database.Maps
{
    public class MapDatabaseCacheProgressEventArgs : EventArgs
    {
        /// <summary>
        ///     The current map sync action being performed.
        /// </summary>
        public string Action { get; }

        /// <summary>
        ///     The map file currently being processed.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        ///     The current index in the sync queue.
        /// </summary>
        public int Index { get; }

        /// <summary>
        ///     The total amount of maps in the sync queue.
        /// </summary>
        public int Total { get; }

        /// <summary>
        /// </summary>
        /// <param name="action"></param>
        /// <param name="fileName"></param>
        /// <param name="index"></param>
        /// <param name="total"></param>
        public MapDatabaseCacheProgressEventArgs(string action, string fileName, int index, int total)
        {
            Action = action;
            FileName = fileName;
            Index = index;
            Total = total;
        }
    }
}
