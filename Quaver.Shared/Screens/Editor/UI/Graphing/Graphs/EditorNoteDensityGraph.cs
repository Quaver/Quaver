/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.API.Maps;
using Quaver.Shared.Screens.Editor.UI.Rulesets;

namespace Quaver.Shared.Screens.Editor.UI.Graphing.Graphs
{
    public class EditorNoteDensityGraph : EditorVisualizationGraph
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="qua"></param>
        /// <param name="ruleset"></param>
        public EditorNoteDensityGraph(EditorVisualizationGraphContainer container, Qua qua, EditorRuleset ruleset)
            : base(container, qua, ruleset) => CreateBars();

        /// <summary>
        ///    Creates the bars for the density graph
        /// </summary>
        private void CreateBars()
        {

        }
    }
}