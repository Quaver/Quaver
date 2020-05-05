using System.Collections.Generic;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs
{
    public class GraphSelectionDropdown : LabelledDropdown
    {
        public GraphSelectionDropdown() : base("GRAPH: ", 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(150, 38), 22, ColorHelper.HexToColor($"#10C8F6")))
        {
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => new List<string>
        {
            "Deviance"
        };
    }
}