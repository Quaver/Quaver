using System.Collections.Generic;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Downloading.UI.Search
{
    public class DownloadModeDropdown : LabelledDropdown
    {
        /// <summary>
        /// </summary>
        private Bindable<DownloadFilterMode> Mode { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="mode"></param>
        public DownloadModeDropdown(Bindable<DownloadFilterMode> mode) : base("MODE: ", 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(120, 38), 22, ColorHelper.HexToColor($"#55ec49"), GetSelectedIndex()))
        {
            Mode = mode;
            Dropdown.ItemSelected += (sender, args) => Mode.Value = (DownloadFilterMode) args.Index;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => new List<string>
        {
            "All",
            "4 Keys",
            "7 Keys",
        };

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex() => 0;
    }

    public enum DownloadFilterMode
    {
        All,
        Keys4,
        Keys7
    }
}