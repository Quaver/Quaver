using Quaver.API.Maps;
using Quaver.Shared.Screens.Editor.UI.Rulesets;

namespace Quaver.Shared.Screens.Editor.UI.Graphing
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
            : base(container, qua, ruleset)
        {
        }
    }
}