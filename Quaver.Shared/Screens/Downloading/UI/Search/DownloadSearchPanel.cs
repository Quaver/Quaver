using System.Collections.Generic;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.API.MapsetSearch;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Downloading.UI.Search
{
    public class DownloadSearchPanel : Sprite
    {
        /// <summary>
        /// </summary>
        private Bindable<string> SearchQuery { get; }

        /// <summary>
        /// </summary>
        private Bindable<DownloadFilterMode> Mode { get; }

        /// <summary>
        /// </summary>
        private Bindable<DownloadFilterRankedStatus> Status { get; }

        /// <summary>
        /// </summary>
        private BindableList<DownloadableMapset> AvailableMapsets { get; }

        /// <summary>
        /// </summary>
        private Bindable<DownloadableMapset> SelectedMapset { get; }

        /// <summary>
        /// </summary>
        private Bindable<DownloadSortBy> SortBy { get; }

        /// <summary>
        ///     Items that are aligned from right to left
        /// </summary>
        private List<Drawable> RightItems { get; } = new List<Drawable>();

        /// <summary>
        /// </summary>
        private DownloadSearchBox SearchBox { get; set; }

        /// <summary>
        /// </summary>
        private DownloadSortByDropdown SortByDropdown { get; set; }

        /// <summary>
        /// </summary>
        private DownloadModeDropdown ModeDropdown { get; set; }

        /// <summary>
        /// </summary>
        private DownloadStatusDropdown StatusDropdown { get; set; }

        /// <summary>
        /// </summary>
        private DownloadDifficultyDropdown DifficultyDropdown { get; set; }

        /// <summary>
        /// </summary>
        private DownloadFoundResults FoundResults { get; set; }

        /// <summary>
        /// </summary>
        private DownloadPanelBanner Banner { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="mode"></param>
        /// <param name="status"></param>
        /// <param name="availableMapsets"></param>
        /// <param name="selectedMapset"></param>
        /// <param name="sortBy"></param>
        public DownloadSearchPanel(Bindable<string> searchQuery, Bindable<DownloadFilterMode> mode,
            Bindable<DownloadFilterRankedStatus> status, BindableList<DownloadableMapset> availableMapsets,
            Bindable<DownloadableMapset> selectedMapset, Bindable<DownloadSortBy> sortBy)
        {
            SearchQuery = searchQuery;
            Mode = mode;
            Status = status;
            AvailableMapsets = availableMapsets;
            SelectedMapset = selectedMapset;
            SortBy = sortBy;

            Size = new ScalableVector2(WindowManager.Width, 88);
            Tint = ColorHelper.HexToColor("#242424");

            CreateBanner();
            //CreateDifficulty();
            CreateRankedStatus();
            CreateMode();
            CreateSearchBar();
            CreateFoundResults();

            AlignRightItems();
        }

        /// <summary>
        /// </summary>
        private void CreateSortBy()
        {
            SortByDropdown = new DownloadSortByDropdown(SortBy);
            RightItems.Add(SortByDropdown);
        }

        /// <summary>
        /// </summary>
        private void CreateRankedStatus()
        {
            StatusDropdown = new DownloadStatusDropdown(Status);
            RightItems.Add(StatusDropdown);
        }

        /// <summary>
        /// </summary>
        private void CreateMode()
        {
            ModeDropdown = new DownloadModeDropdown(Mode);
            RightItems.Add(ModeDropdown);
        }

        /// <summary>
        /// </summary>
        private void CreateSearchBar()
        {
            SearchBox = new DownloadSearchBox(SearchQuery, new ScalableVector2(280, 40))
            {
                AllowCursorMovement = false
            };
            RightItems.Add(SearchBox);
        }

        /// <summary>
        /// </summary>
        private void CreateBanner()
        {
            Banner = new DownloadPanelBanner(new ScalableVector2(960, Height), SelectedMapset) {Parent = this};
        }

        /// <summary>
        /// </summary>
        private void CreateDifficulty()
        {
            DifficultyDropdown = new DownloadDifficultyDropdown();
            RightItems.Add(DifficultyDropdown);
        }

        /// <summary>
        /// </summary>
        private void CreateFoundResults()
        {
            FoundResults = new DownloadFoundResults(AvailableMapsets);
            RightItems.Add(FoundResults);
        }

        /// <summary>
        ///     Aligns the items from right to left
        /// </summary>
        private void AlignRightItems()
        {
            for (var i = 0; i < RightItems.Count; i++)
            {
                var item = RightItems[i];

                item.Parent = this;

                item.Alignment = Alignment.MidRight;

                const int padding = 20;
                var spacing = 36;

                if (i == 0)
                    item.X = -padding;
                else
                    item.X = RightItems[i - 1].X - RightItems[i - 1].Width - spacing;
            }
        }
    }
}