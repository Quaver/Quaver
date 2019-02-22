/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Quaver.Shared.Database.Scores;

namespace Quaver.Shared.Screens.Select.UI.Leaderboard
{
    public struct FetchedScoreStore
    {
        public List<Score> Scores { get; }
        public Score PersonalBest { get; }

        public FetchedScoreStore(List<Score> scores, Score personalBest = null)
        {
            Scores = scores;
            PersonalBest = personalBest;
        }
    }
}
