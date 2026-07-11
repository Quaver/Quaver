using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Helpers;
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
        private Bindable<GameMode> Mode { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="mode"></param>
        public DownloadModeDropdown(Bindable<GameMode> mode) : base(DownloadLocalization.Get("MODE: "), 18, new Dropdown(GetDropdownItems(),
            new ScalableVector2(120, 38), 20, ColorHelper.HexToColor($"#55ec49"), GetSelectedIndex()))
        {
            Mode = mode;
            Dropdown.ItemSelected += (sender, args) => Mode.Value = (GameMode)args.Index;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems()
        {
            var list = new List<string> {
                DownloadLocalization.Get("All")
            };

            foreach (var mode in ModeHelper.AllModes)
            {
                list.Add(DownloadLocalization.Get(ModeHelper.ToLongHand(mode)));
            }

            return list;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex() => 0;
    }
}
