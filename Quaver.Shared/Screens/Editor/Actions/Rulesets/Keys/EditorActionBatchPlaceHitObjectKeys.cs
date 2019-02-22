using System.Collections.Generic;
using osu_database_reader.Components.HitObjects;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Editor.UI.Graphing;
using Quaver.Shared.Screens.Editor.UI.Graphing.Graphs;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Keys
{
    public class EditorActionBatchPlaceHitObjectKeys : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.BatchPlaceHitObjects;

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private EditorScrollContainerKeys Container { get; }

        /// <summary>
        /// </summary>
        private List<HitObjectInfo> HitObjects { get; }

        /// <summary>
        /// </summary>
        /// <param name="workingMap"></param>
        /// <param name="container"></param>
        /// <param name="hitObjects"></param>
        public EditorActionBatchPlaceHitObjectKeys(Qua workingMap, EditorScrollContainerKeys container, List<HitObjectInfo> hitObjects)
        {
            WorkingMap = workingMap;
            Container = container;
            HitObjects = hitObjects;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            var graphContainer = Container.Ruleset.VisualizationGraphs[EditorVisualizationGraphType.Density];
            var graph = graphContainer?.GraphRaw as EditorNoteDensityGraph;

            foreach (var h in HitObjects)
            {
                WorkingMap.HitObjects.Add(h);
                Container.AddHitObjectSprite(h);
                graph?.RefreshSample(h);
            }

            WorkingMap.Sort();
            Container.Ruleset.Screen.SetHitSoundObjectIndex();
            graphContainer?.ForceRecache();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo()
        {
            var graphContainer = Container.Ruleset.VisualizationGraphs[EditorVisualizationGraphType.Density];
            var graph = graphContainer?.GraphRaw as EditorNoteDensityGraph;

            foreach (var h in HitObjects)
            {
                WorkingMap.HitObjects.Remove(h);
                Container.RemoveHitObjectSprite(h);
                graph?.RefreshSample(h);
            }

            WorkingMap.Sort();
            Container.Ruleset.Screen.SetHitSoundObjectIndex();
            graphContainer?.ForceRecache();
        }
    }
}