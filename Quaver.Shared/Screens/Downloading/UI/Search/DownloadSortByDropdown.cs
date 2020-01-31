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
        public DownloadSortByDropdown(Bindable<DownloadSortBy> sortBy) : base("SORT BY: ", 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(180, 38), 22, ColorHelper.HexToColor($"#ffe76b"), GetSelectedIndex()))
        {
            Dropdown.ItemSelected += (sender, args) => sortBy.Value = (DownloadSortBy) args.Index;
        }

        private static List<string> GetDropdownItems() => new List<string>()
        {
            "Newest",
            "Artist",
            "Title",
            "Creator",
            "Bpm",
            "Length",
            "Min Difficulty",
            "Max Difficulty",
            "Long Note %",
            "Play Count",
            "Max Combo"
        };

        private static int GetSelectedIndex() => 0;
    }

    public enum DownloadSortBy
    {
        Newest,
        Artist,
        Title,
        Creator,
        Bpm,
        Length,
        MinDifficulty,
        MaxDifficulty,
        LNs,
        PlayCount,
        MaxCombo
    }
}