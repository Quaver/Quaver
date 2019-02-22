using System.Linq;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Editor.UI.Graphing;
using Quaver.Shared.Screens.Editor.UI.Graphing.Graphs;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Wobble;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal
{
    public class EditorActionAddSliderVelocity : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.AddSliderVelocity;

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private SliderVelocityInfo Velocity { get; }

        /// <summary>
        /// </summary>
        /// <param name="workingMap"></param>
        /// <param name="sv"></param>
        public EditorActionAddSliderVelocity(Qua workingMap, SliderVelocityInfo sv)
        {
            WorkingMap = workingMap;
            Velocity = sv;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            WorkingMap.SliderVelocities.Add(Velocity);
            WorkingMap.SortSliderVelocities();

            var game = GameBase.Game as QuaverGame;
            var screen = game?.CurrentScreen as EditorScreen;
            var ruleset = screen?.Ruleset as EditorRulesetKeys;
            var graph = ruleset?.VisualizationGraphs[EditorVisualizationGraphType.Tick].GraphRaw as EditorTickGraph;
            graph?.AddSliderVelocityLine(Velocity);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo()
        {
            WorkingMap.SliderVelocities.Remove(Velocity);
            WorkingMap.SortSliderVelocities();

            var game = GameBase.Game as QuaverGame;
            var screen = game?.CurrentScreen as EditorScreen;
            var ruleset = screen?.Ruleset as EditorRulesetKeys;
            var graph = ruleset?.VisualizationGraphs[EditorVisualizationGraphType.Tick].GraphRaw as EditorTickGraph;
            graph?.RemoveSliderVelocityLine(Velocity);
        }
    }
}