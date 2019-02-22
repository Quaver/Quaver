using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Editor.UI.Graphing;
using Quaver.Shared.Screens.Editor.UI.Graphing.Graphs;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Wobble;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal
{
    public class EditorActionRemoveTimingPoints : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.RemoveTimingPoints;

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private List<TimingPointInfo> TimingPoints { get; }

        /// <summary>
        /// </summary>
        public EditorActionRemoveTimingPoints(Qua workingMap, List<TimingPointInfo> tps)
        {
            WorkingMap = workingMap;
            TimingPoints = tps;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            var game = GameBase.Game as QuaverGame;
            var screen = game?.CurrentScreen as EditorScreen;
            var ruleset = screen?.Ruleset as EditorRulesetKeys;
            var graph = ruleset?.VisualizationGraphs[EditorVisualizationGraphType.Tick].GraphRaw as EditorTickGraph;

            TimingPoints.ForEach(x =>
            {
                WorkingMap.TimingPoints.Remove(x);
                graph?.RemoveTimingPointLine(x);
            });

            WorkingMap.SortTimingPoints();
            ruleset?.ScrollContainer.Timeline.CompletelyReinitialize();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo()
        {
            var game = GameBase.Game as QuaverGame;
            var screen = game?.CurrentScreen as EditorScreen;
            var ruleset = screen?.Ruleset as EditorRulesetKeys;
            var graph = ruleset?.VisualizationGraphs[EditorVisualizationGraphType.Tick].GraphRaw as EditorTickGraph;

            TimingPoints.ForEach(x =>
            {
                WorkingMap.TimingPoints.Add(x);
                graph?.AddTimingPointLine(x);
            });

            WorkingMap.SortTimingPoints();
            ruleset?.ScrollContainer.Timeline.CompletelyReinitialize();
        }
    }
}