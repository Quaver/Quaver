using System.Collections.Generic;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.API.MapsetSearch;
using Quaver.Shared.Screens.Downloading.UI.Filter.Items;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Filter
{
    public class DownloadFilterContainer : Sprite
    {
        /// <summary>
        /// </summary>
        private Bindable<DownloadableMapset> SelectedMapset { get; }

        /// <summary>
        /// </summary>
        private BindableFloat MinDifficulty { get; }

        /// <summary>
        /// </summary>
        private BindableFloat MaxDifficulty { get; }

        /// <summary>
        /// </summary>
        private BindableFloat MinBpm { get; }

        /// <summary>
        /// </summary>
        private BindableFloat MaxBpm { get; }

        /// <summary>
        /// </summary>
        private BindableInt MinLength { get; }

        /// <summary>
        /// </summary>
        private BindableInt MaxLength { get; }

        /// <summary>
        /// </summary>
        private BindableInt MinLongNotePercent { get; }

        /// <summary>
        /// </summary>
        private BindableInt MaxLongNotePercent { get; }

        /// <summary>
        /// </summary>
        private BindableInt MinPlayCount { get; }

        /// <summary>
        /// </summary>
        private BindableInt MaxPlayCount { get; }

        /// <summary>
        /// </summary>
        private Bindable<string> MinUploadDate { get; }

        /// <summary>
        /// </summary>
        private Bindable<string> MaxUploadDate { get; }

        /// <summary>
        /// </summary>
        private Bindable<string> MinLastUpdateDate { get; }

        /// <summary>
        /// </summary>
        private Bindable<string> MaxLastUpdateDate { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> DisplayOwnedMapsets { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> ReverseSort { get; }

        /// <summary>
        /// </summary>
        private BindableInt MinCombo { get; }

        /// <summary>
        /// </summary>
        private BindableInt MaxCombo { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Header { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Container { get; set; }

        /// <summary>
        /// </summary>
        private DownloadFilterBanner Banner { get; set; }

        /// <summary>
        /// </summary>
        private List<DownloadFilterTableItem> TableItems { get; set; }

        /// <summary>
        /// </summary>
        private TextboxTabControl TabControl { get; set; }

        /// <summary>
        /// </summary>
        public DownloadFilterContainer(BindableFloat minDiff, BindableFloat maxDiff, BindableFloat minBpm, BindableFloat maxBpm,
            BindableInt minLength, BindableInt maxLength, BindableInt minlns, BindableInt maxlns, BindableInt minPlayCount,
            BindableInt maxPlayCount, Bindable<string> minUploadDate, Bindable<string> maxUploadDate,
            Bindable<DownloadableMapset> selectedMapset, Bindable<bool> displayOwnedMapsets, Bindable<bool> reverseSort,
            Bindable<string> minLastUpdateDate, Bindable<string> maxLastUpdateDate, BindableInt minCombo, BindableInt maxCombo)
        {
            SelectedMapset = selectedMapset;
            MinDifficulty = minDiff;
            MaxDifficulty = maxDiff;
            MinBpm = minBpm;
            MaxBpm = maxBpm;
            MinLength = minLength;
            MaxLength = maxLength;
            MinLongNotePercent = minlns;
            MaxLongNotePercent = maxlns;
            MinPlayCount = minPlayCount;
            MaxPlayCount = maxPlayCount;
            MinUploadDate = minUploadDate;
            MaxUploadDate = maxUploadDate;
            DisplayOwnedMapsets = displayOwnedMapsets;
            ReverseSort = reverseSort;
            MinLastUpdateDate = minLastUpdateDate;
            MaxLastUpdateDate = maxLastUpdateDate;
            MinCombo = minCombo;
            MaxCombo = maxCombo;

            Alpha = 0f;
            Size = new ScalableVector2(564, 838);
            AutoScaleHeight = true;

            CreateHeader();
            CreateContainer();
            CreateBanner();
            CreateTable();
        }

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            Header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "FILTER OPTIONS", 26)
            {
                Parent = this,
            };
        }

        /// <summary>
        /// </summary>
        private void CreateContainer()
        {
            const int spacing = 10;

            Container = new Sprite
            {
                Parent = this,
                Y = Header.Y + Header.Height + spacing,
                Size = new ScalableVector2(Width, Height - Header.Height - spacing),
                Tint = ColorHelper.HexToColor("#242424")
            };

            Container.AddBorder(ColorHelper.HexToColor("#0FBAE5"), 2);
        }

        /// <summary>
        /// </summary>
        private void CreateBanner()
        {
            Banner = new DownloadFilterBanner(SelectedMapset, new ScalableVector2(Container.Width - 4, 154))
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 2
            };
        }

        /// <summary>
        /// </summary>
        private void CreateTable()
        {
            var tableWidth = (int) Width - 4;

            TableItems = new List<DownloadFilterTableItem>()
            {
                new DownloadFilterTableItemCheckbox(tableWidth, "Display Owned Mapsets", DisplayOwnedMapsets),
                new DownloadFilterTableItemDifficulty(tableWidth, MinDifficulty, MaxDifficulty),
                new DownloadFilterTableItemBpm(tableWidth, MinBpm, MaxBpm),
                new DownloadFilterTableLength(tableWidth, MinLength, MaxLength),
                new DownloadFilterTableItemLongNotePercent(tableWidth, MinLongNotePercent, MaxLongNotePercent),
                new DownloadFilterTableItemMaxCombo(tableWidth, MinCombo, MaxCombo),
                new DownloadFilterTableItemDate(tableWidth, "Upload Date", MinUploadDate, MaxUploadDate),
                new DownloadFilterTableItemDate(tableWidth, "Last Update Date", MinLastUpdateDate, MaxLastUpdateDate),
            };

            TabControl = new TextboxTabControl(new List<Textbox>()) { Parent = this };

            for (var i = 0; i < TableItems.Count; i++)
            {
                var item = TableItems[i];

                item.Alignment = Alignment.TopCenter;
                item.Height = (Container.Height - Banner.Height - 2) / TableItems.Count;

                item.Y = Banner.Height + i * item.Height;
                item.Tint = i % 2 == 0 ? ColorHelper.HexToColor("#363636") : ColorHelper.HexToColor("#242424");

                // Loop backwards to add textboxes to the tab control, because they're created from
                // max to min
                for (var j = item.Children.Count - 1; j >= 0; j--)
                {
                    if (item.Children[j] is Textbox textbox)
                        TabControl.AddTextbox(textbox);
                }
            }

            for (var i = TableItems.Count - 1; i >= 0; i--)
                TableItems[i].Parent = Container;
        }
    }
}