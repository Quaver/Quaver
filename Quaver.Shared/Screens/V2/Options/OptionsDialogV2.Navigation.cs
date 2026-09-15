using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.V2.Options.Catalog;
using Quaver.Shared.Screens.V2.Options.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options
{
    /// <summary>
    ///     The category list in the rail and the subcategory list next to it.
    /// </summary>
    internal sealed partial class OptionsDialogV2
    {
        private Texture2D OptionsIcons { get; set; }

        private Texture2D SearchRailIcon { get; set; }

        private Texture2D SearchResetIcon { get; set; }

        private Texture2D RecentlyChangedIcon { get; set; }

        private ScrollContainer CategoryNavigationScroll { get; set; }

        private FlexContainer CategoryNavigationList { get; set; }

        private ScrollContainer SubcategoryNavigationScroll { get; set; }

        private FlexContainer SubcategoryNavigationList { get; set; }

        /// <summary>
        ///     Includes the "Recently Changed" button while no search is running.
        /// </summary>
        private List<OptionsCategoryButton> CategoryButtons { get; } = new List<OptionsCategoryButton>();

        private List<OptionsSubcategoryButton> SubcategoryButtons { get; } = new List<OptionsSubcategoryButton>();

        /// <summary>
        ///     Takes the place of the "Recently Changed" button while a search is running.
        /// </summary>
        private OptionsSearchRailButton SearchRailButton { get; set; }

        /// <summary>
        ///     The menu opens on "Recently Changed", or on the first category when nothing was changed yet.
        /// </summary>
        private OptionsCategoryDefinition SelectedCategory { get; set; } = OptionsRecentlyChangedTracker.HasEntries()
            ? OptionsNavigationCatalog.RecentlyChanged
            : OptionsNavigationCatalog.Categories[0];

        private string SelectedSubcategoryKey { get; set; } = OptionsNavigationCatalog.AllLocalizationKey;

        /// <summary>
        ///     The sizes used for the last navigation layout, to skip it when nothing changed.
        /// </summary>
        private float LastNavigationRailWidth { get; set; } = -1;

        private float LastNavigationBodyHeight { get; set; } = -1;

        private float LastNavigationCategoryWidth { get; set; } = -1;

        private void LoadIcons()
        {
            var config = Config.Categories;

            OptionsIcons = OptionsIconAtlas.Load(Skin, config.IconAtlas);
            SearchRailIcon = Skin.LoadTexture(config.SearchIcon, UserInterface.OptionsV2SearchIcon);
            SearchResetIcon = Skin.LoadTexture(config.SearchResetIcon, UserInterface.OptionsV2ResetIcon);
            RecentlyChangedIcon = Skin.LoadTexture(config.RecentlyChangedIcon, UserInterface.OptionsV2RecentlyChangedIcon);
        }

        /// <summary>
        ///     Builds the rail's list: a pinned entry, a line, then the categories. The pinned entry is
        ///     "Recently Changed", or the search entry while a search is running.
        /// </summary>
        private void CreateCategoryNavigation()
        {
            OptionsDrawableCleanup.DestroyTree(CategoryNavigationScroll);
            CategoryButtons.Clear();
            SearchRailButton = null;

            var config = Config.Categories;
            var font = FontManager.GetWobbleFont(config.Font);
            var contentHeight = GetRailContentHeight();

            CategoryNavigationScroll = CreateNavigationScroll(RailOverlay, contentHeight);
            CategoryNavigationList = CreateNavigationList(contentHeight);

            if (SearchActive)
            {
                SearchRailButton = new OptionsSearchRailButton(SearchRailIcon, SearchResetIcon, font, config,
                    () => PendingNavigationAction = () => ClearSearch());
                AddToList(CategoryNavigationList, SearchRailButton, config.ButtonHeight);
            }
            else
            {
                var recentlyChanged = OptionsNavigationCatalog.RecentlyChanged;
                var button = new OptionsCategoryButton(recentlyChanged,
                    new TextureRegion(RecentlyChangedIcon, RecentlyChangedIcon.Bounds), font, config,
                    (sender, args) => PendingNavigationAction = () => SelectCategory(recentlyChanged));
                AddToList(CategoryNavigationList, button, config.ButtonHeight);
                CategoryButtons.Add(button);
            }

            AddToList(CategoryNavigationList, new OptionsRailSeparator(config), config.SearchSeparatorThickness);

            foreach (var definition in OptionsNavigationCatalog.Categories)
            {
                var button = new OptionsCategoryButton(definition,
                    OptionsIconAtlas.GetRegion(OptionsIcons, definition.Icon.Value), font, config,
                    (sender, args) => PendingNavigationAction = () => SelectCategory(definition));
                AddToList(CategoryNavigationList, button, config.ButtonHeight);
                CategoryButtons.Add(button);
            }

            CategoryNavigationScroll.AddContainedDrawable(CategoryNavigationList);
            ApplyCategorySelection();
            UpdateNavigationLayout(true);
            UpdateCategoryLabelProgress();
        }

        /// <summary>
        ///     Builds the subcategory list for the selected category: "All", then each subcategory.
        /// </summary>
        private void CreateSubcategoryNavigation()
        {
            OptionsDrawableCleanup.DestroyTree(SubcategoryNavigationScroll);
            SubcategoryButtons.Clear();

            var config = Config.Categories;
            var font = FontManager.GetWobbleFont(config.Font);
            var keys = OptionsContentCatalog.GetSubcategories(SelectedCategory.Id)
                .Select(group => group.SubcategoryLocalizationKey)
                .Prepend(OptionsNavigationCatalog.AllLocalizationKey)
                .ToArray();
            var contentHeight = GetListContentHeight(keys.Length);

            SubcategoryNavigationScroll = CreateNavigationScroll(CategoryPanel, contentHeight);
            SubcategoryNavigationList = CreateNavigationList(contentHeight);

            foreach (var key in keys)
            {
                var button = new OptionsSubcategoryButton(key, font, config,
                    (sender, args) => PendingNavigationAction = () => SelectSubcategory(key));
                AddToList(SubcategoryNavigationList, button, config.ButtonHeight);
                SubcategoryButtons.Add(button);
            }

            SubcategoryNavigationScroll.AddContainedDrawable(SubcategoryNavigationList);
            ApplySubcategorySelection();
            UpdateNavigationLayout(true);
        }

        private void SelectCategory(OptionsCategoryDefinition category)
        {
            SelectedCategory = category;
            SelectedSubcategoryKey = OptionsNavigationCatalog.AllLocalizationKey;

            if (SearchActive)
                ClearSearch(false);

            ApplyCategorySelection();
            CreateSubcategoryNavigation();
            RebuildContent();

            if (RailExpanded)
                SetRailExpanded(false, true);
        }

        private void SelectSubcategory(string localizationKey)
        {
            SelectedSubcategoryKey = localizationKey;

            if (SearchActive)
                ClearSearch(false);

            ApplySubcategorySelection();
            RebuildContent();
        }

        /// <summary>
        ///     While a search is running no category is selected, and all of them are greyed out.
        /// </summary>
        private void ApplyCategorySelection()
        {
            foreach (var button in CategoryButtons)
            {
                button.SetSelected(!SearchActive && button.Definition.Id == SelectedCategory.Id);
                button.SetDimmed(SearchActive);
            }
        }

        private void ApplySubcategorySelection()
        {
            foreach (var button in SubcategoryButtons)
                button.SetSelected(button.LocalizationKey == SelectedSubcategoryKey);
        }

        /// <summary>
        ///     Fits both lists to their panels. Does nothing when no panel changed size, unless forced.
        /// </summary>
        private void UpdateNavigationLayout(bool force = false)
        {
            if (CategoryNavigationScroll == null || SubcategoryNavigationScroll == null)
                return;

            if (!force && Math.Abs(LastNavigationRailWidth - RailOverlay.Width) < 0.001f &&
                Math.Abs(LastNavigationBodyHeight - Body.Height) < 0.001f &&
                Math.Abs(LastNavigationCategoryWidth - CategoryPanel.Width) < 0.001f)
                return;

            LastNavigationRailWidth = RailOverlay.Width;
            LastNavigationBodyHeight = Body.Height;
            LastNavigationCategoryWidth = CategoryPanel.Width;

            var inset = Config.Categories.PanelInset;

            var railWidth = Math.Max(1, RailOverlay.Width - inset * 2);
            var toggleTop = Math.Max(inset, Body.Height - Config.Rail.ToggleInset - RailToggle.Height);
            var railHeight = Math.Max(1, toggleTop - inset * 2);
            LayoutNavigationScroll(CategoryNavigationScroll, CategoryNavigationList, railWidth, railHeight,
                GetRailContentHeight());

            var categoryWidth = Math.Max(1, CategoryPanel.Width - inset * 2);
            var categoryHeight = Math.Max(1, CategoryPanel.Height - inset * 2);
            LayoutNavigationScroll(SubcategoryNavigationScroll, SubcategoryNavigationList, categoryWidth,
                categoryHeight, GetListContentHeight(SubcategoryButtons.Count));
        }

        private void LayoutNavigationScroll(ScrollContainer scroll, FlexContainer list, float viewportWidth,
            float viewportHeight, float contentHeight)
        {
            scroll.Position = new ScalableVector2(Config.Categories.PanelInset, Config.Categories.PanelInset);
            scroll.Size = new ScalableVector2(viewportWidth, viewportHeight);
            scroll.ContentContainer.Size = new ScalableVector2(viewportWidth, Math.Max(viewportHeight, contentHeight));
            list.Size = new ScalableVector2(viewportWidth, Math.Max(1, contentHeight));
            list.RefreshLayout();
        }

        private ScrollContainer CreateNavigationScroll(Drawable parent, float contentHeight) =>
            new ScrollContainer(new ScalableVector2(1, 1), new ScalableVector2(1, Math.Max(1, contentHeight)))
            {
                Parent = parent,
                Position = new ScalableVector2(Config.Categories.PanelInset, Config.Categories.PanelInset),
                Tint = Color.Transparent,
                InputEnabled = true,
                AllowScrollbarDragging = true,
                ScrollSpeed = 80,
                Scrollbar =
                {
                    Width = Config.Categories.ScrollbarWidth,
                    Tint = SkinV2Color.Parse(Config.Categories.ScrollbarColor)
                }
            };

        private FlexContainer CreateNavigationList(float contentHeight) => new FlexContainer
        {
            Size = new ScalableVector2(1, Math.Max(1, contentHeight)),
            Direction = FlexDirection.Column,
            AlignItems = FlexAlignItems.Stretch,
            RowGap = Config.Categories.RowSpacing
        };

        private static void AddToList(FlexContainer list, Drawable item, float height)
        {
            item.Parent = list;
            list.SetItemOptions(item, new FlexItemOptions { Basis = height, Shrink = 0 });
        }

        /// <summary>
        ///     The height of the rail's list: the categories, plus the pinned entry and the line below it.
        /// </summary>
        private float GetRailContentHeight() =>
            GetListContentHeight(OptionsNavigationCatalog.Categories.Count) + Config.Categories.ButtonHeight +
            Config.Categories.SearchSeparatorThickness + Config.Categories.RowSpacing * 2;

        private float GetListContentHeight(int itemCount)
        {
            if (itemCount <= 0)
                return 1;

            return itemCount * Config.Categories.ButtonHeight + (itemCount - 1) * Config.Categories.RowSpacing;
        }
    }
}
