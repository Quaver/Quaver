/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using SQLite;

namespace Quaver.Shared.Database.Settings
{
    /// <summary>
    ///     A row table that consists of the versions of the difficulty/rating/score calculators
    ///     that we have, so we can update the map database cache with new values whenever
    ///     we upgrade them.
    /// </summary>
    public class QuaverSettings
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        public string VersionDifficultyProcessorKeys { get; set; }

        public string VersionRatingProcessorKeys { get; set; }

        public string VersionScoreProcessorKeys { get; set; }
    }
}
