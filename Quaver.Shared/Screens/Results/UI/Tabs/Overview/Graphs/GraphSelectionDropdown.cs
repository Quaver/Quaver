using System.Collections.Generic;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs
{
    public class GraphSelectionDropdown : LabelledDropdown
    {
        public GraphSelectionDropdown() : base("GRAPH: ", 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(150, 38), 22, ColorHelper.HexToColor($"#10C8F6"), GetSelectedIndex()))
        {
            Dropdown.ItemSelected += OnItemSelected;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => new List<string>
        {
            "Deviance",
            "Accuracy",
            "Health",
            "Rating"
        };

        /// <summary>
        ///     Retrieves the index of the selected value
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            if (ConfigManager.ResultGraph != null)
                return (int) ConfigManager.ResultGraph.Value;

            return 0;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemSelected(object sender, DropdownClickedEventArgs e)
        {
            if (ConfigManager.ResultGraph == null)
                return;

            ConfigManager.ResultGraph.Value = (ResultGraphs) e.Index;
        }
    }
}