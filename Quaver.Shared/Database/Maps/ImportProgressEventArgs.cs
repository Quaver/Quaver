/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;

namespace Quaver.Shared.Database.Maps
{
    public class ImportProgressEventArgs : EventArgs
    {
        /// <summary>
        ///     The current import phase.
        /// </summary>
        public string Status { get; }

        /// <summary>
        ///     Additional context for the current import phase.
        /// </summary>
        public string Details { get; }

        /// <summary>
        ///     The number of completed items in the current phase.
        /// </summary>
        public int Completed { get; }

        /// <summary>
        ///     The total number of items in the current phase.
        /// </summary>
        public int Total { get; }

        /// <summary>
        ///     If true, the current phase has a determinate progress value.
        /// </summary>
        public bool HasProgress => Total > 0;

        /// <summary>
        ///     The current phase progress percentage.
        /// </summary>
        public double Percentage => HasProgress ? Math.Clamp(Completed * 100d / Total, 0, 100) : 0;

        /// <summary>
        /// </summary>
        /// <param name="status"></param>
        /// <param name="details"></param>
        /// <param name="completed"></param>
        /// <param name="total"></param>
        public ImportProgressEventArgs(string status, string details = "", int completed = 0, int total = 0)
        {
            Status = status;
            Details = details;
            Completed = completed;
            Total = total;
        }

        /// <summary>
        ///     Reports an import progress update if the completed count should be shown.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="status"></param>
        /// <param name="details"></param>
        /// <param name="completed"></param>
        /// <param name="total"></param>
        /// <param name="checkIfShouldReport"></param>
        public static void Report(Action<ImportProgressEventArgs>? progress, string status, string details = "", int completed = 0, int total = 0, bool checkIfShouldReport = true)
        {
            if (checkIfShouldReport && !ShouldReport(completed, total))
                return;

            progress?.Invoke(new ImportProgressEventArgs(status, details, completed, total));
        }

        /// <summary>
        ///     Prevents large imports from scheduling thousands of UI text updates.
        /// </summary>
        /// <param name="completed"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        private static bool ShouldReport(int completed, int total)
        {
            if (total <= 0)
                return false;

            var interval = Math.Max(1, total / 100);
            return completed == 1 || completed == total || completed % interval == 0;
        }
    }
}
