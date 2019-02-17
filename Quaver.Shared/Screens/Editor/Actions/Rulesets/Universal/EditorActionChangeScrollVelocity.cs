using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.Shared.Screens.Editor.UI.Dialogs.SV;
using Quaver.Shared.Screens.Editor.UI.Graphing;
using Quaver.Shared.Screens.Editor.UI.Graphing.Graphs;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Wobble;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal
{
    public class EditorActionChangeScrollVelocity : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.ChangeScrollVelocity;

        /// <summary>
        /// </summary>
        public List<EditorScrollVelocityChangeInfo> Changes { get; }

        /// <summary>
        /// </summary>
        public Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        /// <param name="workingMap"></param>
        /// <param name="changes"></param>
        public EditorActionChangeScrollVelocity(Qua workingMap, List<EditorScrollVelocityChangeInfo> changes)
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
                x.Info.Multiplier = x.NewMultiplier;
                graph?.MoveSliderVelocityLine(x.Info);
            });

            WorkingMap.SortSliderVelocities();
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
                x.Info.Multiplier = x.OriginalMultiplier;
                graph?.MoveSliderVelocityLine(x.Info);
            });

            WorkingMap.SortSliderVelocities();
        }
    }
}