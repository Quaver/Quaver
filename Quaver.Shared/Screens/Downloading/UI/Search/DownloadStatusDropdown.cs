using System.Collections.Generic;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Downloading.UI.Search
{
    public class DownloadStatusDropdown : LabelledDropdown
    {
        /// <summary>
        /// </summary>
        private Bindable<DownloadFilterRankedStatus> Status { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="status"></param>
        public DownloadStatusDropdown(Bindable<DownloadFilterRankedStatus> status) : base("STATUS: ", 22,
            new Dropdown(GetDropdownItems(), new ScalableVector2(150, 38), 22, Colors.MainAccent, GetSelectedIndex()))
        {
            Status = status;

            Dropdown.ItemSelected += (sender, args) => Status.Value = (DownloadFilterRankedStatus) args.Index;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => new List<string>
        {
            "All",
            "Unranked",
            "Ranked",
        };

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex() => 2;
    }

    public enum DownloadFilterRankedStatus
    {
        All,
        Unranked,
        Ranked,
    }
}