/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Editor.UI.Graphing;
using Quaver.Shared.Screens.Editor.UI.Graphing.Graphs;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Keys
{
    public class EditorActionPlaceHitObjectKeys : IEditorAction
    {
        /// <summary>
        /// </summary>
        private EditorScrollContainerKeys Container { get; }

        /// <summary>
        ///     The HitObject to place.
        /// </summary>
        private HitObjectInfo HitObject { get; }

        /// <summary>
        /// </summary>
        private Qua WorkingMap => Container.Ruleset.WorkingMap;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.PlaceHitObject;

        /// <summary>
        /// </summary>
        public EditorActionPlaceHitObjectKeys(EditorScrollContainerKeys container, HitObjectInfo hitObject)
        {
            Container = container;
            HitObject = hitObject;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            WorkingMap.HitObjects.Add(HitObject);
            WorkingMap.Sort();
            Container.AddHitObjectSprite(HitObject);
            Container.Ruleset.Screen.SetHitSoundObjectIndex();

            var graphContainer = Container.Ruleset.VisualizationGraphs[EditorVisualizationGraphType.Density];
            var graph = graphContainer?.GraphRaw as EditorNoteDensityGraph;
            graph?.RefreshSample(HitObject);
            graphContainer?.ForceRecache();
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public void Undo() => new EditorActionDeleteHitObjectKeys(Container, HitObject).Perform();
    }
}
