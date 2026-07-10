using System.Collections.Generic;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Downloading.UI.Search
{
    public class DownloadSortByDropdown : LabelledDropdown
    {
        public DownloadSortByDropdown(Bindable<DownloadSortBy> sortBy) : base(DownloadLocalization.Get("SORT BY: "), 18, new Dropdown(GetDropdownItems(),
            new ScalableVector2(180, 38), 18, ColorHelper.HexToColor($"#ffe76b"), GetSelectedIndex()))
        {
            Dropdown.ItemSelected += (sender, args) => sortBy.Value = (DownloadSortBy) args.Index;
        }

        private static List<string> GetDropdownItems() => new List<string>()
        {
            DownloadLocalization.Get("Newest"),
            DownloadLocalization.Get("Date Submitted"),
            DownloadLocalization.Get("Length"),
            DownloadLocalization.Get("Difficulty"),
            DownloadLocalization.Get("Max Combo"),
            DownloadLocalization.Get("BPM"),
            DownloadLocalization.Get("LN %"),
            DownloadLocalization.Get("Play Count"),
        };

        private static int GetSelectedIndex() => 0;
    }

    public enum DownloadSortBy
    {
        DateLastUpdated,
        DateSubmitted,
        Length,
        DifficultyRating,
        MaxCombo,
        Bpm,
        LongNotePercentage,
        PlayCount,
    }
}
