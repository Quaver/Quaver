using System.Collections.Generic;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Downloading.UI.Search
{
    public class DownloadSortOrderDropdown : Dropdown
    {
        public DownloadSortOrderDropdown(Bindable<bool> sortBy) : base(GetDropdownItems(),
            new ScalableVector2(150, 38), 22, ColorHelper.HexToColor($"#ffe76b"), sortBy.Value ? 1 : 0)
        {
            ItemSelected += (sender, args) => sortBy.Value = args.Index > 0;
        }

        private static List<string> GetDropdownItems() => new List<string>()
        {
            "Ascending",
            "Descending"
        };
    }
}
