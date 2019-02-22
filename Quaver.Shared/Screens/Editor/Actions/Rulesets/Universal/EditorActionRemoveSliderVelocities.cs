using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Editor.UI.Graphing;
using Quaver.Shared.Screens.Editor.UI.Graphing.Graphs;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Wobble;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal
{
    public class EditorActionRemoveSliderVelocities : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.RemoveSliderVelocities;

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private List<SliderVelocityInfo> Velocities { get; }

        /// <summary>
        /// </summary>
        public EditorActionRemoveSliderVelocities(Qua workingMap, List<SliderVelocityInfo> svs)
        {
            WorkingMap = workingMap;
            Velocities = svs;
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

            Velocities.ForEach(x =>
            {
                WorkingMap.SliderVelocities.Remove(x);
                graph?.RemoveSliderVelocityLine(x);
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

            Velocities.ForEach(x =>
            {
                WorkingMap.SliderVelocities.Add(x);
                graph?.AddSliderVelocityLine(x);
            });

            WorkingMap.SortSliderVelocities();
        }
    }
}