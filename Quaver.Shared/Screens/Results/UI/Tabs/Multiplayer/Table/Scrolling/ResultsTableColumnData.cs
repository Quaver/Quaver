using Microsoft.Xna.Framework;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Multiplayer.Table.Scrolling
{
    public struct ResultsTableColumnData
    {
        public string ColumnText { get; }

        public string Value { get; }

        public Color Tint { get; }

        public ResultsTableColumnData(string columnText, string value, Color tint)
        {
            ColumnText = columnText;
            Value = value;
            Tint = tint;
        }
    }
}