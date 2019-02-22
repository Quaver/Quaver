using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Editor.UI.Graphing;
using Quaver.Shared.Screens.Editor.UI.Graphing.Graphs;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Wobble;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal
{
    public class EditorActionAddTimingPoint : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.AddTimingPoint;

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private TimingPointInfo TimingPoint { get; }

        /// <summary>
        /// </summary>
        /// <param name="workingMap"></param>
        /// <param name="tp"></param>
        public EditorActionAddTimingPoint(Qua workingMap, TimingPointInfo tp)
        {
            WorkingMap = workingMap;
            TimingPoint = tp;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            WorkingMap.TimingPoints.Add(TimingPoint);
            WorkingMap.SortTimingPoints();

            var game = GameBase.Game as QuaverGame;
            var screen = game?.CurrentScreen as EditorScreen;
            var ruleset = screen?.Ruleset as EditorRulesetKeys;
            var graph = ruleset?.VisualizationGraphs[EditorVisualizationGraphType.Tick].GraphRaw as EditorTickGraph;
            graph?.AddTimingPointLine(TimingPoint);
            ruleset?.ScrollContainer.Timeline.CompletelyReinitialize();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo()
        {
            WorkingMap.TimingPoints.Remove(TimingPoint);
            WorkingMap.SortTimingPoints();

            var game = GameBase.Game as QuaverGame;
            var screen = game?.CurrentScreen as EditorScreen;
            var ruleset = screen?.Ruleset as EditorRulesetKeys;
            var graph = ruleset?.VisualizationGraphs[EditorVisualizationGraphType.Tick].GraphRaw as EditorTickGraph;
            graph?.RemoveTimingPointLine(TimingPoint);

            ruleset?.ScrollContainer.Timeline.CompletelyReinitialize();
        }
    }
}