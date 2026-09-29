using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;
using Quaver.Shared.Input.Global;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.V2.Options.Model;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Screens.V2.UI.Filters;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Window;
using static System.Net.Mime.MediaTypeNames;
using static Quaver.Shared.Graphics.Overlays.V2Hub.Users.UsersSection;

namespace Quaver.Shared.Screens.V2.Options
{
    public class OptionsV2Dialog : DialogScreen
    {
        private const float LayoutGap = 10f;

        private bool InputRecorderWasFocused { get; set; }
        private bool RecentOptionsDirty { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Panel { get; set; }
        private FlexContainer Layout { get; set; }

        // Left content

        /// <summary>
        /// Placeholder component that keep the layout in place when growing / shrinking the Icon List
        /// </summary>
        private FlexContainer IconListContainer;
        /// <summary>
        /// Component that handle the section displayed when the Icon List is shrunk
        /// </summary>
        private ScrollContainer SectionListScroll;
        private FlexContainer SectionListLayout;
        /// <summary>
        /// Component that will actually grow / shrink when clicking on the IconToggle button
        /// </summary>
        private NineSliceSprite IconList;
        private readonly Dictionary<OptionCategory, RoundedButton> CategoryIcons = new Dictionary<OptionCategory, RoundedButton>();
        private RoundedButton IconRecentSearch;
        private SpriteTextPlus IconRecentSearchText;
        private RoundedButton IconToggle;
        private bool IsIconListExpanded;

        private OptionCategory? CurrentCategory { get; set; } = null;
        private const string AllSectionsName = "Screen_Options_SectionAll";
        private static readonly (OptionCategory? Category, string Name) AllSection = (null, AllSectionsName);
        private (OptionCategory? Category, string Name)? SelectedCategorySection { get; set; }
        private readonly Dictionary<(OptionCategory? Category, string Name), RoundedButton> CategorySectionButtons = new Dictionary<(OptionCategory?, string), RoundedButton>();
        private readonly List<(OptionCategory Category, string Name, FlexContainer Header)> SectionStarts = new List<(OptionCategory, string, FlexContainer)>();

        private FlexContainer MainMenuLayout { get; set; }
        private FlexContainer HeaderMenuLayout { get; set; }
        private FlexContainer MenuColumnsLayout { get; set; }
        private FlexContainer MenuLayout { get; set; }

        // Right content
        private V2FilterSearchTextbox SearchBox;
        private Bindable<string> SearchQuery { get; } = new Bindable<string>(string.Empty);
        private bool IsSearching => !string.IsNullOrWhiteSpace(SearchQuery.Value);
        private V2Dropdown<string> PressetDropdown { get; set; }
        private FlexContainer MainContentLayout { get; set; }
        private FlexContainer HeaderContentLayout { get; set; }
        private FlexContainer ContentLayout { get; set; }
        private Sprite ContentBackground { get; set; }
        private ScrollContainer OptionsScroll { get; set; }
        private FlexContainer OptionsRows { get; set; }

        public OptionsV2Dialog() : base(0.75f)
        {
            CreateLayouts();

            CreateContent();
            CreateLeftMenu();
            CreateSectionMenu();
            CreateMainContent();

            ConfigManager.RecentlyChangedOptions.ValueChanged += OnRecentlyChangedOptionsChanged;
            SearchQuery.ValueChanged += OnSearchQueryChanged;
            WindowManager.VirtualScreenSizeChanged += OnVirtualScreenSizeChanged;
            UpdateLayout();

            SelectCategory(OptionCategory.Video);
        }

        public override void CreateContent()
        {
            // Top left content
            CreateTwoSidedSprite(HeaderMenuLayout, 18,
                new ScalableVector2(89, 40), ColorHelper.FromHex("#4B5973"), LocalizationManager.Get("Screen_Options_Title"), Color.White,
                new ScalableVector2(229, 40), ColorHelper.FromHex("#181E25"), LocalizationManager.Get("Screen_Options_TitleDescription"), ColorHelper.FromHex("#D9E3F4")
            );

            // Top right content
            var searchStyle = new V2FilterFieldStyle
            {
                Height = 40,
                SearchIconSize = 18,
                SearchIconInset = 10,
                CornerRadius = SkinV2BorderRadiusConfig.Normal,
                BackgroundColor = ColorHelper.FromHex("#181E25"),
                TextColor = Color.White,
                PlaceholderColor = ColorHelper.FromHex("#4B5973"),
                CursorColor = Color.White
            };

            SearchBox = new V2FilterSearchTextbox(SearchQuery, LocalizationManager.Get("Screen_Options_Searchforoptions"), FontManager.GetWobbleFont(Fonts.InterBold), 18, searchStyle, 726)
            {
                Parent = HeaderContentLayout
            };

            var dropdownStyle = new SkinV2DropdownConfig
            {
                Height = 40,
                ItemHeight = 40,
                FontSize = 18,
                TriggerColor = "#181E25FF",
                ItemColor = "#181E25FF",
                HoverColor = "#354451FF",
                SelectedItemColor = "#6B83B2FF",
                TextColor = "#8CAFEAFF",
                IconColor = "#EBF3FFFF",
                CornerRadius = SkinV2BorderRadiusConfig.Normal
            };

            PressetDropdown = new V2Dropdown<string>(242, new Bindable<string>("test1"), new DropdownEntry<string>[]
            {
                new DropdownOption<string>("test 1", LocalizationManager.Get("Screen_Selection_All")),
                new DropdownOption<string>("test 2", LocalizationManager.Get("Screen_Selection_Friends")),
                new DropdownOption<string>("test 3", LocalizationManager.Get("Screen_Selection_Country"))
            }, FontManager.GetWobbleFont(Fonts.InterBold), dropdownStyle, Container)
            {
                Parent = HeaderContentLayout
            };
        }

        private void CreateLeftMenu()
        {
            var menuBackground = new Sprite
            {
                Parent = MenuLayout,
                Size = MenuLayout.Size,
                Tint = ColorHelper.FromHex("#273038")
            };
            menuBackground.Image = RoundedRectTextureCache.Get(menuBackground.Width, menuBackground.Height, 6f);
            MenuLayout.SetItemOptions(menuBackground, new FlexItemOptions { Basis = 0, Grow = 1 });

            MenuColumnsLayout = new FlexContainer
            {
                Parent = menuBackground,
                Size = menuBackground.Size,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch,
                ColumnGap = 10
            };

            IconListContainer = new FlexContainer
            {
                Parent = MenuColumnsLayout,
                Size = new ScalableVector2(60, MenuLayout.Height)
            };
            MenuColumnsLayout.SetItemOptions(IconListContainer, new FlexItemOptions { Basis = IconListContainer.Width });

            SectionListScroll = new ScrollContainer(new ScalableVector2(1, Math.Max(1f, MenuLayout.Height - 10)), new ScalableVector2(1, 1))
            {
                Parent = MenuColumnsLayout,
                Tint = Color.Transparent,
                AllowScrollbarDragging = true,
                AllowMiddleMouseDragging = true,
                CapturesMouseWheelInput = true,
                Scrollbar = { Width = 4, Tint = ColorHelper.FromHex("#6B83B2") }
            };
            SectionListScroll.Scrollbar.UsePreviousSpriteBatchOptions = true;
            MenuColumnsLayout.SetItemOptions(SectionListScroll, new FlexItemOptions { Basis = 0, Grow = 1, AlignSelf = FlexAlignSelf.FlexEnd });

            SectionListLayout = new FlexContainer
            {
                Size = new ScalableVector2(1, 1),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = 10
            };
            SectionListScroll.AddContainedDrawable(SectionListLayout);

            IconList = new NineSliceSprite(RoundedRectTextureCache.Get(20f, 20f, 6f), new SliceMargins(6))
            {
                Parent = Panel, // Must be drawn after Layout to render on top of it
                Alignment = Alignment.TopLeft,
                Size = IconListContainer.Size,
                Tint = ColorHelper.FromHex("#181E25")
            };

            var buttonHeight = 10;
            IconRecentSearch = new RoundedButton
            {
                Parent = IconList,
                Position = new ScalableVector2(10, buttonHeight),
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(40, 40),
                CornerRadius = 6f,
                Tint = ColorHelper.FromHex("#273038"),
                SetChildrenAlpha = false
            };
            IconRecentSearchText = CreateSpriteText(IconRecentSearch, LocalizationManager.Get("Screen_Options_RecentlyChanged"), 18, ColorHelper.FromHex("#D0DBED"), Alignment.MidLeft, 0, new ScalableVector2(50, 0));
            IconRecentSearch.SetIcon(GetOptionsIcon(10), new Vector2(30, 30));
            IconRecentSearch.Icon.Alignment = Alignment.MidLeft;
            IconRecentSearch.Icon.X = 5;
            IconRecentSearch.Clicked += (s, e) => SelectCategory(null);

            buttonHeight += (int)IconRecentSearch.Height + 10;

            var separator = new Sprite
            {
                Parent = IconList,
                Position = new ScalableVector2(10, buttonHeight),
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(40, 2),
                Tint = ColorHelper.FromHex("#D9E3F440"),
                SetChildrenAlpha = false
            };

            buttonHeight += (int)separator.Height + 10;

            foreach (var category in (OptionCategory[])Enum.GetValues(typeof(OptionCategory)))
            {
                var iconButton = new RoundedButton
                {
                    Parent = IconList,
                    Position = new ScalableVector2(10, buttonHeight),
                    Alignment = Alignment.TopLeft,
                    Size = new ScalableVector2(40, 40),
                    CornerRadius = 6f,
                    Tint = ColorHelper.FromHex("#273038"),
                    SetChildrenAlpha = false
                };

                // Contains
                // Item1: icon row index
                // Item2: localisation key
                var iconRowInfo = category switch
                {
                    OptionCategory.Video => (1, "Screen_Options_Video"),
                    OptionCategory.Audio => (2, "Screen_Options_Audio"),
                    OptionCategory.Gameplay => (3, "Screen_Options_Gameplay"),
                    OptionCategory.Skin => (4, "Screen_Options_Skin"),
                    OptionCategory.Input => (5, "Screen_Options_Input"),
                    OptionCategory.Miscellaneous => (6, "Screen_Options_Miscellaneous"),
                    OptionCategory.Advanced => (7, "Screen_Options_Advanced")
                };
                CreateSpriteText(iconButton, LocalizationManager.Get(iconRowInfo.Item2), 18, ColorHelper.FromHex("#D0DBED"), Alignment.MidLeft, 0, new ScalableVector2(50, 0));
                iconButton.SetIcon(GetOptionsIcon(iconRowInfo.Item1), new Vector2(30, 30));
                iconButton.Icon.Alignment = Alignment.MidLeft;
                iconButton.Icon.X = 5;
                iconButton.Clicked += (s, e) => SelectCategory(category);
                CategoryIcons.Add(category, iconButton);

                buttonHeight += 50;
            }

            IconToggle = new RoundedButton
            {
                Parent = IconList,
                Alignment = Alignment.BotLeft,
                Position = new ScalableVector2(10, -10),
                Size = new ScalableVector2(40, 40),
                CornerRadius = 6f,
                Tint = ColorHelper.FromHex("#4B5973"),
            };
            IconToggle.SetIcon(GetOptionsIcon(9), new Vector2(30, 30));
            IconToggle.Clicked += (s, e) => ExpandIconList(!IsIconListExpanded);
        }

        private void CreateSectionMenu()
        {
            foreach (var button in CategorySectionButtons.Values)
                button.Destroy();
            CategorySectionButtons.Clear();

            SelectedCategorySection = null;
            CreateSectionMenuButton(AllSection, LocalizationManager.Get(AllSectionsName));

            var options = GetVisibleOptions();
            var sections = options.DistinctBy(option => (option.Category, option.SectionName)).ToList();
            foreach (var section in sections)
            {
                var name = LocalizationManager.Get(section.SectionName);
                if (!CurrentCategory.HasValue && sections.Count(other => other.SectionName == section.SectionName) > 1)
                    name = $"{LocalizationManager.Get($"Screen_Options_{section.Category}")} · {name}";

                CreateSectionMenuButton((section.Category, section.SectionName), name);
            }
            UpdateSectionListLayout();
            SectionListScroll.ScrollTo(0, 1);
            SetSectionHighlight(AllSection);
        }
        private void CreateSectionMenuButton((OptionCategory? Category, string Name) section, string label)
        {
            var sectionButton = new RoundedButton
            {
                Parent = SectionListLayout,
                Size = new ScalableVector2(236, 40),
                Tint = ColorHelper.FromHex("#181E25"),
                CornerRadii = new RoundedRectCornerRadii(6, 6, 6, 6),
                UsePreviousSpriteBatchOptions = true
            };

            sectionButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), label, 18, Color.White);
            sectionButton.Label.Alignment = Alignment.MidLeft;
            sectionButton.Label.X = 10;
            SectionListLayout.SetItemOptions(sectionButton, new FlexItemOptions { Basis = sectionButton.Height, Shrink = 0, AlignSelf = FlexAlignSelf.FlexStart });
            sectionButton.Clicked += (s, e) => OnSectionButtonClicked(section);

            CategorySectionButtons.Add(section, sectionButton);
        }

        private void CreateMainContent()
        {
            ContentBackground = new Sprite
            {
                Parent = ContentLayout,
                Size = ContentLayout.Size,
                Tint = ColorHelper.FromHex("#273038")
            };
            ContentLayout.SetItemOptions(ContentBackground, new FlexItemOptions { Basis = 0, Grow = 1 });

            OptionsScroll = new ScrollContainer(new ScalableVector2(1, 1), new ScalableVector2(1, 1))
            {
                Parent = ContentBackground,
                Position = new ScalableVector2(10, 10),
                Tint = Color.Transparent,
                AllowScrollbarDragging = true,
                AllowMiddleMouseDragging = true,
                EasingType = Easing.OutQuint,
                CapturesMouseWheelInput = true,
                Scrollbar = { Width = 4, Tint = ColorHelper.FromHex("#6B83B2") }
            };
            OptionsScroll.Scrollbar.UsePreviousSpriteBatchOptions = true;
            SectionListScroll.ScrollSpeed = OptionsScroll.ScrollSpeed;
            SectionListScroll.EasingType = OptionsScroll.EasingType;
            SectionListScroll.TimeToCompleteScroll = OptionsScroll.TimeToCompleteScroll;
            SectionListScroll.TimeToCompleteMiddleMouseScroll = OptionsScroll.TimeToCompleteMiddleMouseScroll;

            OptionsRows = new FlexContainer
            {
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = 10
            };
            OptionsScroll.AddContainedDrawable(OptionsRows);
        }

        private void RefreshOptionsRows()
        {
            RecentOptionsDirty = false;
            SectionStarts.Clear();
            foreach (var child in OptionsRows.Children.ToList())
                child.Destroy();

            var options = GetVisibleOptions();
            UpdateSearchPresentation(options.Count);

            foreach (var option in options)
            {
                var control = OptionsList.CreateControl(option, Container);
                if (control == null)
                    continue;

                if (SectionStarts.Count == 0 || SectionStarts[SectionStarts.Count - 1].Category != option.Category || SectionStarts[SectionStarts.Count - 1].Name != option.SectionName)
                {
                    var header = CreateSectionHeader(option.Category, option.SectionName);
                    SectionStarts.Add((option.Category, option.SectionName, header));
                }

                var row = new OptionsRow(option)
                {
                    Parent = OptionsRows,
                    Size = new ScalableVector2(1, 54)
                };
                OptionsRows.SetItemOptions(row, new FlexItemOptions { Basis = row.Height, Shrink = 0 });
                row.SetControl(control);
            }

            UpdateOptionsContentLayout();
            OptionsScroll.ScrollTo(0, 1);
            SetSectionHighlight(AllSection);
        }

        private IReadOnlyList<OptionsDefinition> GetVisibleOptions() => CurrentCategory.HasValue
            ? OptionsList.All.Where(option => option.Category == CurrentCategory.Value).ToList()
            : IsSearching ? OptionsList.Search(SearchQuery.Value) : OptionsList.Recent;

        private void UpdateSearchPresentation(int resultCount)
        {
            if (IsSearching)
            {
                IconRecentSearchText.Text = GetSearchResultsText(resultCount);
                IconRecentSearch.SetIcon(GetOptionsIcon(0), new Vector2(30, 30));
                UpdateResultsCount(resultCount);
            }
            else
            {
                IconRecentSearchText.Text = LocalizationManager.Get("Screen_Options_RecentlyChanged");
                IconRecentSearch.SetIcon(GetOptionsIcon(10), new Vector2(30, 30));
                SearchBox.SetResultsText(string.Empty);
            }
        }

        private FlexContainer CreateSectionHeader(OptionCategory category, string sectionName)
        {
            var header = new FlexContainer
            {
                Parent = OptionsRows,
                Size = new ScalableVector2(1, 40),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch
            };

            header.Width = CreateTwoSidedSprite(header, 18,
                new ScalableVector2(1, header.Height), ColorHelper.FromHex("#4B5973"), LocalizationManager.Get($"Screen_Options_{category}"), Color.White,
                new ScalableVector2(1, header.Height), ColorHelper.FromHex("#181E25"), LocalizationManager.Get(sectionName), ColorHelper.FromHex("#D9E3F4"),
                fitToText: true);
            OptionsRows.SetItemOptions(header, new FlexItemOptions { Basis = header.Height, Shrink = 0, AlignSelf = FlexAlignSelf.FlexStart });

            return header;
        }

        private void ExpandIconList(bool expanded)
        {
            IsIconListExpanded = expanded;

            IconList.ClearAnimations();

            IconToggle.SetIcon(GetOptionsIcon(expanded ? 8 : 9), new Vector2(30, 30));

            var targetWidth = expanded ? MenuLayout.Width + 1 : IconListContainer.Width; // Don't ask why +1, idk why it doesn't grow properly
            IconList.ChangeWidthTo((int)Math.Round(targetWidth), expanded ? Easing.OutCubic : Easing.InCubic, 200);
            IconList.Children.ForEach(sprite =>
            {
                sprite.ChangeWidthTo((int)Math.Round(targetWidth - 20), expanded ? Easing.OutCubic : Easing.InCubic, 200);
                sprite.Children.ForEach(child => {
                    if (child is SpriteTextPlus spriteText)
                    {
                        spriteText.ClearAnimations(AnimationProperty.Alpha);
                        spriteText.FadeTo(expanded ? 1f : 0f, expanded ? Easing.OutCubic : Easing.InCubic, 200);

                    }
                });
            });
        }

        private static TextureRegion GetOptionsIcon(int row)
        {
            if (row < 0 || row >= 11)
                throw new ArgumentOutOfRangeException(nameof(row));

            return new TextureRegion(UserInterface.OptionsIconsSheet, new Rectangle(0, row * 84, 60, 60));
        }

        private float CreateTwoSidedSprite(FlexContainer parentLayout, int textSize,
            ScalableVector2 sprite1Size, Color sprite1Color, string sprite1Text, Color sprite1TextColor,
            ScalableVector2 sprite2Size, Color sprite2Color, string sprite2Text, Color sprite2TextColor,
            bool fitToText = false)
        {
            var sprite1 = new Sprite
            {
                Parent = parentLayout,
                Size = sprite1Size,
                Tint = sprite1Color
            };

            var sprite1Label = CreateSpriteText(sprite1, sprite1Text, textSize, sprite1TextColor,
                fitToText ? Alignment.MidLeft : Alignment.MidCenter,
                position: fitToText ? new ScalableVector2(15, 0) : null);
            if (fitToText)
                sprite1.Width = sprite1Label.Width + 30f;
            sprite1.Image = RoundedRectTextureCache.Get(sprite1.Width, sprite1.Height, new RoundedRectCornerRadii(6, 0, 0, 6));
            parentLayout.SetItemOptions(sprite1, new FlexItemOptions { Basis = sprite1.Width, Grow = fitToText ? 0 : 1, Shrink = fitToText ? 0 : 1 });

            var sprite2 = new Sprite
            {
                Parent = parentLayout,
                Size = sprite2Size,
                Tint = sprite2Color
            };

            var sprite2Label = CreateSpriteText(sprite2, sprite2Text, textSize, sprite2TextColor,
                fitToText ? Alignment.MidLeft : Alignment.MidCenter,
                position: fitToText ? new ScalableVector2(15, 0) : null);
            if (fitToText)
                sprite2.Width = sprite2Label.Width + 30f;
            sprite2.Image = RoundedRectTextureCache.Get(sprite2.Width, sprite2.Height, new RoundedRectCornerRadii(0, 6, 6, 0));
            parentLayout.SetItemOptions(sprite2, new FlexItemOptions { Basis = sprite2.Width, Grow = fitToText ? 0 : 1, Shrink = fitToText ? 0 : 1 });

            return sprite1.Width + sprite2.Width;
        }

        private SpriteTextPlus CreateSpriteText(Sprite sprite, string text, int size, Color color, Alignment alignment = Alignment.MidCenter, float alpha = 1f, ScalableVector2? position = null, WobbleFontStore fontUsed = null)
        {
            return new SpriteTextPlus(fontUsed ?? FontManager.GetWobbleFont(Fonts.InterBold), text, size)
            {
                Parent = sprite,
                Position = position ?? new ScalableVector2(0, 0),
                Alignment = alignment,
                Tint = color,
                Alpha = alpha
            };
        }

        private void CreateLayouts()
        {
            // Main panel
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            Container.Size = Size;

            Panel = new Sprite
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(1268, 622),
                Tint = ColorHelper.FromHex("#424141")
            };
            Panel.Image = RoundedRectTextureCache.Get(Panel.Width, Panel.Height, 6f);

            Layout = new FlexContainer
            {
                Parent = Panel,
                Position = new ScalableVector2(0, 0),
                Size = Panel.Size,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch,
                ColumnGap = LayoutGap
            };

            // Left side of the option dialog
            MainMenuLayout = new FlexContainer
            {
                Parent = Layout,
                Size = new ScalableVector2(318, Panel.Height),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = LayoutGap
            };
            Layout.SetItemOptions(MainMenuLayout, new FlexItemOptions { Basis = MainMenuLayout.Width });

            HeaderMenuLayout = new FlexContainer
            {
                Parent = MainMenuLayout,
                Size = new ScalableVector2(MainMenuLayout.Width, 40),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch
            };
            MainMenuLayout.SetItemOptions(HeaderMenuLayout, new FlexItemOptions { Basis = HeaderMenuLayout.Height });

            MenuLayout = new FlexContainer
            {
                Parent = MainMenuLayout,
                Size = new ScalableVector2(MainMenuLayout.Width, MainMenuLayout.Height - HeaderMenuLayout.Height - LayoutGap),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch
            };
            MainMenuLayout.SetItemOptions(MenuLayout, new FlexItemOptions { Basis = 0, Grow = 1 });

            // Right part of the option dialog
            MainContentLayout = new FlexContainer
            {
                Parent = Layout,
                Size = new ScalableVector2(Panel.Width - MainMenuLayout.Width - LayoutGap, Panel.Height),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = LayoutGap
            };
            Layout.SetItemOptions(MainContentLayout, new FlexItemOptions { Basis = 0, Grow = 1 });

            HeaderContentLayout = new FlexContainer
            {
                Parent = MainContentLayout,
                Size = new ScalableVector2(MainContentLayout.Width, 40),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch,
                ColumnGap = LayoutGap
            };
            MainContentLayout.SetItemOptions(HeaderContentLayout, new FlexItemOptions { Basis = HeaderContentLayout.Height });

            ContentLayout = new FlexContainer
            {
                Parent = MainContentLayout,
                Size = new ScalableVector2(MainContentLayout.Width, MainContentLayout.Height - HeaderContentLayout.Height - LayoutGap),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch
            };
            MainContentLayout.SetItemOptions(ContentLayout, new FlexItemOptions { Basis = 0, Grow = 1 });
        }

        private string GetSearchResultsText(int count) => LocalizationManager.Get(count == 1 ? "Screen_Options_SearchResult" : "Screen_Options_SearchResults", count);
        private void UpdateResultsCount(int count) => SearchBox.SetResultsText(GetSearchResultsText(count));
        private void OnVirtualScreenSizeChanged(object sender, WindowVirtualScreenSizeChangedEventArgs e) => UpdateLayout();

        private void OnRecentlyChangedOptionsChanged(object sender, BindableValueChangedEventArgs<string> e)
        {
            if (!CurrentCategory.HasValue && !IsSearching)
                RecentOptionsDirty = true;
        }

        private void OnSearchQueryChanged(object sender, BindableValueChangedEventArgs<string> e)
        {
            if (IsSearching && CurrentCategory.HasValue)
                SelectCategory(null);
            else if (!CurrentCategory.HasValue)
            {
                CreateSectionMenu();
                RefreshOptionsRows();
            }
        }

        private void SelectCategory(OptionCategory? category)
        {
            if (CurrentCategory == category)
                return;

            // Update button color
            SetIconColor(CurrentCategory.HasValue ? CategoryIcons[CurrentCategory.Value] : IconRecentSearch, ColorHelper.FromHex("#273038"), ColorHelper.FromHex("#D0DBED"));

            CurrentCategory = category;
            if (category.HasValue && !string.IsNullOrEmpty(SearchQuery.Value))
                SearchQuery.Value = string.Empty;

            SetIconColor(CurrentCategory.HasValue ? CategoryIcons[CurrentCategory.Value] : IconRecentSearch, ColorHelper.FromHex("#6B83B2"), ColorHelper.FromHex("#FFFFFF"));

            // Create buttons for that category
            CreateSectionMenu();
            RefreshOptionsRows();

        }
        private void OnSectionButtonClicked((OptionCategory? Category, string Name) section)
        {
            if (section == AllSection)
            {
                OptionsScroll.ScrollTo(0, 800);
                return;
            }

            var start = SectionStarts.FirstOrDefault(item => item.Category == section.Category && item.Name == section.Name);
            if (start.Header != null)
                OptionsScroll.ScrollTo(-start.Header.Y, 800);
        }

        private void SetSectionHighlight((OptionCategory? Category, string Name) section)
        {
            if (SelectedCategorySection == section)
                return;

            SelectedCategorySection = section;
            foreach (var button in CategorySectionButtons)
                button.Value.Tint = ColorHelper.FromHex(button.Key == section ? "#6B83B2" : "#181E25");
        }

        private void UpdateSectionHighlight()
        {
            var scrollableHeight = OptionsScroll.ContentContainer.Height - OptionsScroll.Height;
            var scrollTop = Math.Max(0, -OptionsScroll.CurrentY);
            if (SectionStarts.Count == 0 || scrollableHeight <= 1 || scrollTop <= 1)
            {
                SetSectionHighlight(AllSection);
                return;
            }

            if (scrollTop >= scrollableHeight - 1)
            {
                var last = SectionStarts[SectionStarts.Count - 1];
                SetSectionHighlight((last.Category, last.Name));
                return;
            }

            var activeSection = SectionStarts[0];
            foreach (var section in SectionStarts)
            {
                if (section.Header.Y > scrollTop + 10)
                    break;
                activeSection = section;
            }

            SetSectionHighlight((activeSection.Category, activeSection.Name));
        }

        private void SetIconColor(RoundedButton button, Color buttonColor, Color textColor)
        {
            button.Tint = buttonColor;

            button.Children.ForEach(child =>
            {
                if (child is SpriteTextPlus textSprite)
                    textSprite.Tint = textColor;
            });
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            if (RecentOptionsDirty && MouseManager.CurrentState.LeftButton == ButtonState.Released && !IsOptionInputFocused(OptionsRows))
            {
                CreateSectionMenu();
                RefreshOptionsRows();
            }

            InputRecorderWasFocused = IsInputRecorderFocused(ContentLayout);
            SectionListScroll.InputEnabled = !IsIconListExpanded && SectionListScroll.IsHovered();
            foreach (var button in CategorySectionButtons.Values)
                button.IsInteractionEnabled = SectionListScroll.InputEnabled;
            OptionsScroll.InputEnabled = OptionsScroll.IsHovered();
            base.Update(gameTime);
            UpdateSectionHighlight();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (InputRecorderWasFocused || IsInputRecorderFocused(ContentLayout))
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
            {
                DialogManager.Dismiss(this);
                return;
            }

            if (!MouseManager.IsUniqueClick(MouseButton.Left))
                return;

            if (!Panel.IsHovered())
                DialogManager.Dismiss(this);
        }

        private static bool IsInputRecorderFocused(Drawable drawable) => drawable is InputRecorderV2 { Focused: true } || drawable.Children.Any(IsInputRecorderFocused);

        private static bool IsOptionInputFocused(Drawable drawable) =>
            drawable is InputRecorderV2 { Focused: true } || drawable is Textbox { Focused: true } || drawable.Children.Any(IsOptionInputFocused);

        private void UpdateLayout()
        {
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            Container.Size = Size;

            Panel.Size = new ScalableVector2(Math.Max(1f, Math.Min(1268f, WindowManager.Width - 32f)), Math.Max(1f, Math.Min(622f, WindowManager.Height - 32f)));
            Panel.Image = RoundedRectTextureCache.Get(Panel.Width, Panel.Height, 6f);

            Layout.Size = Panel.Size;

            Layout.RefreshLayout();
            MainMenuLayout.RefreshLayout();
            MainContentLayout.RefreshLayout();
            HeaderMenuLayout.RefreshLayout();
            MenuLayout.RefreshLayout();
            SectionListScroll.Height = Math.Max(1f, MenuLayout.Height - 10);
            MenuColumnsLayout.RefreshLayout();
            UpdateSectionListLayout();
            HeaderContentLayout.RefreshLayout();
            ContentLayout.RefreshLayout();
            UpdateOptionsContentLayout();

            SyncIconList();
        }
        private void UpdateSectionListLayout()
        {
            var buttonsHeight = CategorySectionButtons.Count * 40f + Math.Max(0, CategorySectionButtons.Count - 1) * SectionListLayout.RowGap;
            SectionListLayout.Size = new ScalableVector2(Math.Max(1f, SectionListScroll.Width), Math.Max(SectionListScroll.Height, buttonsHeight));
            SectionListScroll.ContentContainer.Size = SectionListLayout.Size;
            SectionListLayout.RefreshLayout();
        }
        private void UpdateOptionsContentLayout()
        {
            ContentBackground.Image = RoundedRectTextureCache.Get(ContentBackground.Width, ContentBackground.Height, 6f);


            OptionsScroll.Size = new ScalableVector2(ContentBackground.Width - 10, ContentBackground.Height - 10 * 2);

            var rowsHeight = OptionsRows.Children.Sum(row => row.Height) + Math.Max(0, OptionsRows.Children.Count - 1) * OptionsRows.RowGap;
            OptionsRows.Size = new ScalableVector2(ContentBackground.Width - 10 * 2, Math.Max(ContentBackground.Height - 10 * 2, rowsHeight));
            OptionsScroll.ContentContainer.Size = OptionsRows.Size;
            OptionsRows.RefreshLayout();
            foreach (var section in SectionStarts)
                section.Header.RefreshLayout();
            foreach (var row in OptionsRows.Children.OfType<OptionsRow>())
                row.RefreshLayout();
        }
        private void SyncIconList()
        {
            var offset = IconListContainer.AbsolutePosition - Panel.AbsolutePosition;
            var width = IsIconListExpanded ? MenuLayout.Width : IconListContainer.Width;

            IconList.ClearAnimations();
            IconList.Position = new ScalableVector2(offset.X, offset.Y);
            IconList.Size = new ScalableVector2(Math.Max(1f, width), Math.Max(1f, MenuLayout.Height));
        }


        public override void Destroy()
        {
            SearchBox.Focused = false;
            PressetDropdown.CloseImmediately();

            ConfigManager.RecentlyChangedOptions.ValueChanged -= OnRecentlyChangedOptionsChanged;
            SearchQuery.ValueChanged -= OnSearchQueryChanged;
            SearchQuery.Dispose();
            WindowManager.VirtualScreenSizeChanged -= OnVirtualScreenSizeChanged;

            base.Destroy();
        }
    }

}
