using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Screens.V2.Options.Catalog;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     The scrolling list of section headers and option rows inside the content panel.
    ///     Rows outside the visible area are hidden, so long categories stay fast.
    /// </summary>
    internal sealed class OptionsContentList
    {
        /// <summary>
        ///     How many rows past the visible area stay active, so rows are ready before they scroll in.
        /// </summary>
        private const float CullMarginRows = 2;

        private Drawable Panel { get; }

        private SkinV2OptionsRowConfig Config { get; }

        private SkinV2SharedConfig SharedConfig { get; }

        /// <summary>
        ///     The layer that dropdown menus open into.
        /// </summary>
        private Container DropdownOverlayHost { get; }

        private ScrollContainer Scroll { get; set; }

        private FlexContainer List { get; set; }

        /// <summary>
        ///     The headers, rows and dividers in the list, in order.
        /// </summary>
        private IReadOnlyList<Drawable> Entries { get; set; } = Array.Empty<Drawable>();

        private float ContentHeight { get; set; }

        private float LastPanelWidth { get; set; } = -1;

        private float LastPanelHeight { get; set; } = -1;

        internal OptionsContentList(Drawable panel, SkinV2OptionsRowConfig config, SkinV2SharedConfig sharedConfig,
            Container dropdownOverlayHost)
        {
            Panel = panel;
            Config = config;
            SharedConfig = sharedConfig;
            DropdownOverlayHost = dropdownOverlayHost;
        }

        /// <summary>
        ///     Replaces the list with a header and rows for each group.
        /// </summary>
        internal void Show(IReadOnlyList<OptionsRowGroup> groups)
        {
            OptionsDrawableCleanup.DestroyTree(Scroll);
            Scroll = null;
            List = null;
            Entries = Array.Empty<Drawable>();
            ContentHeight = 0;

            if (groups.Count == 0)
                return;

            var labelFont = FontManager.GetWobbleFont(Config.LabelFont);
            var headerFont = FontManager.GetWobbleFont(Config.SectionHeaderFont);
            var entries = new List<Drawable>();

            foreach (var group in groups)
            {
                entries.Add(new OptionsSectionHeaderV2(headerFont, Config, GetCategoryHeaderText(group.Category),
                    LocalizationManager.Get(group.SubcategoryLocalizationKey)));

                foreach (var row in group.Rows)
                    entries.Add(OptionsRowFactory.CreateRow(row, labelFont, Config, SharedConfig, DropdownOverlayHost));
            }

            ContentHeight = entries.Sum(entry => GetEntryHeight(entry)) +
                            Math.Max(0, entries.Count - 1) * Config.RowSpacing;

            Scroll = new ScrollContainer(new ScalableVector2(1, 1), new ScalableVector2(1, Math.Max(1, ContentHeight)))
            {
                Parent = Panel,
                Tint = Color.Transparent,
                InputEnabled = true,
                AllowScrollbarDragging = true,
                ScrollSpeed = 80,
                Scrollbar =
                {
                    Width = Config.ScrollbarWidth,
                    Tint = SkinV2Color.Parse(Config.ScrollbarColor)
                }
            };

            List = new FlexContainer
            {
                Size = new ScalableVector2(1, Math.Max(1, ContentHeight)),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = Config.RowSpacing
            };

            foreach (var entry in entries)
            {
                entry.Parent = List;
                List.SetItemOptions(entry, new FlexItemOptions
                {
                    Basis = GetEntryHeight(entry),
                    Shrink = 0,
                    AlignSelf = entry is OptionsSectionHeaderV2 ? FlexAlignSelf.FlexStart : FlexAlignSelf.Auto
                });
                DrawableViewportCuller.Prepare(entry);
            }

            Entries = entries;
            Scroll.AddContainedDrawable(List);
            UpdateLayout(true);
            UpdateCulling();
        }

        /// <summary>
        ///     Fits the list to the panel. Does nothing when the panel size did not change, unless forced.
        /// </summary>
        internal void UpdateLayout(bool force = false)
        {
            if (Scroll == null)
                return;

            if (!force && Math.Abs(LastPanelWidth - Panel.Width) < 0.001f &&
                Math.Abs(LastPanelHeight - Panel.Height) < 0.001f)
                return;

            LastPanelWidth = Panel.Width;
            LastPanelHeight = Panel.Height;

            var inset = Config.ContentInset;
            var width = Math.Max(1, Panel.Width - inset * 2);
            var height = Math.Max(1, Panel.Height - inset * 2);

            Scroll.Position = new ScalableVector2(inset, inset);
            Scroll.Size = new ScalableVector2(width, height);
            Scroll.ContentContainer.Size = new ScalableVector2(width, Math.Max(height, ContentHeight));
            List.Size = new ScalableVector2(width, Math.Max(1, ContentHeight));
            List.RefreshLayout();
        }

        /// <summary>
        ///     Hides the entries outside the visible area. Call this before the rows update each frame,
        ///     so a row that scrolls into view is updated in the same frame.
        /// </summary>
        internal void UpdateCulling()
        {
            if (Scroll == null)
                return;

            DrawableViewportCuller.Apply(Entries, Scroll.ScreenRectangle, Config.RowHeight * CullMarginRows);
        }

        private float GetEntryHeight(Drawable entry) =>
            entry is OptionsDividerV2 ? Config.DividerThickness : Config.RowHeight;

        /// <summary>
        ///     The left half of a section header.
        /// </summary>
        private static string GetCategoryHeaderText(OptionsCategoryId category) =>
            LocalizationManager.Get(OptionsNavigationCatalog.Get(category).LocalizationKey + "Settings");
    }
}
