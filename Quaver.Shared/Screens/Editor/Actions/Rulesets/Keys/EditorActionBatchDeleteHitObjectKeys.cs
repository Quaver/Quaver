/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using osu_database_reader.Components.HitObjects;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Editor.UI.Graphing;
using Quaver.Shared.Screens.Editor.UI.Graphing.Graphs;
using Quaver.Shared.Screens.Editor.UI.Rulesets;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.HitObjects;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Keys
{
    public class EditorActionBatchDeleteHitObjectKeys : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.BatchDeleteHitObjects;

        /// <summary>
        /// </summary>
        private EditorRuleset Ruleset { get; }

        /// <summary>
        /// </summary>
        private EditorScrollContainerKeys Container { get; }

        /// <summary>
        /// </summary>
        private List<DrawableEditorHitObject> HitObjects { get; }

        /// <summary>
        /// </summary>
        public EditorActionBatchDeleteHitObjectKeys(EditorRuleset ruleset, EditorScrollContainerKeys container, List<DrawableEditorHitObject> hitObjects)
        {
            Ruleset = ruleset;
            Container = container;
            HitObjects = hitObjects;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            var graphContainer = Container.Ruleset.VisualizationGraphs[EditorVisualizationGraphType.Density];

            lock (Container.HitObjects)
            {
                foreach (var h in HitObjects)
                {
                    Ruleset.WorkingMap.HitObjects.Remove(h.Info);
                    Container.RemoveHitObjectSprite(h.Info);

                    var graph = graphContainer?.GraphRaw as EditorNoteDensityGraph;
                    graph?.RefreshSample(h.Info);
                }
            }

            Ruleset.WorkingMap.Sort();
            Container.Ruleset.Screen.SetHitSoundObjectIndex();
            graphContainer?.ForceRecache();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo()
        {
            var graphContainer = Container.Ruleset.VisualizationGraphs[EditorVisualizationGraphType.Density];

            lock (Container.HitObjects)
            {
                foreach (var h in HitObjects)
                {
                    Ruleset.WorkingMap.HitObjects.Add(h.Info);
                    Container.AddHitObjectSprite(h.Info);

                    var graph = graphContainer?.GraphRaw as EditorNoteDensityGraph;
                    graph?.RefreshSample(h.Info);
                }
            }

            Ruleset.WorkingMap.Sort();
            Container.Ruleset.Screen.SetHitSoundObjectIndex();
            graphContainer?.ForceRecache();
        }
    }
}