using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.Shared.Screens.Editor.UI.Dialogs.Timing;
using Quaver.Shared.Screens.Editor.UI.Graphing;
using Quaver.Shared.Screens.Editor.UI.Graphing.Graphs;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Wobble;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal
{
    public class EditorActionChangeTimingPoint : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.ChangeTimingPoint;

        /// <summary>
        /// </summary>
        public List<EditorTimingPointChangeInfo> Changes { get; }

        /// <summary>
        /// </summary>
        public Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        /// <param name="workingMap"></param>
        /// <param name="changes"></param>
        public EditorActionChangeTimingPoint(Qua workingMap, List<EditorTimingPointChangeInfo> changes)
        {
            WorkingMap = workingMap;
            Changes = changes;
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

            Changes.ForEach(x =>
            {
                x.Info.StartTime = x.NewTime;
                x.Info.Bpm = x.NewBpm;
                graph?.MoveTimingPointLine(x.Info);
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

            Changes.ForEach(x =>
            {
                x.Info.StartTime = x.OriginalTime;
                x.Info.Bpm = x.OriginalBpm;
                graph?.MoveTimingPointLine(x.Info);
            });

            WorkingMap.SortTimingPoints();
            ruleset?.ScrollContainer.Timeline.CompletelyReinitialize();
        }
    }
}