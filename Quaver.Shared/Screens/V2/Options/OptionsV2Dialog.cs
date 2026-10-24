using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.V2.Options.Modal;
using Quaver.Shared.Screens.V2.Options.Model;
using Quaver.Shared.Screens.V2.SkinEditor;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Screens.V2.UI.Filters;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using SkiaSharp;
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
using Wobble.Logging;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.Options
{
    public class OptionsV2Dialog : DialogScreen
    {
        private SkinStoreV2Lease Skin { get; }
        private bool OwnsSkin { get; }
        private bool PreviewMode { get; }
        private SkinV2Config RootConfig { get; }
        private SkinV2OptionsConfig Config => RootConfig.Screens.Options;
        private float LayoutGap => Config.Layout.Gap;
        private static readonly Guid CreatePresetEntryId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        private static readonly Guid ManagePresetEntryId = Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffffe");
        private static readonly Guid SavePresetEntryId = Guid.Parse("ffffffff-ffff-ffff-ffff-fffffffffffd");
        private static readonly Color UnsavedPresetColor = new(255, 224, 50);
        private static readonly Color SavePresetActionColor = ColorHelper.FromHex("#27B06E");
        private static readonly Color ManagePresetActionColor = ColorHelper.FromHex("#F2994A");
        private static readonly Color CreatePresetActionColor = ColorHelper.FromHex("#9B51E0");

        private bool InputRecorderWasFocused { get; set; }
        private bool RecentOptionsDirty { get; set; }
        private bool PresetDirtyCheckPending { get; set; }
        private bool AppliedPresetHasChanges { get; set; }

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
        private HorizontalClippingContainer SectionListClip;
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
        private readonly HashSet<(OptionCategory Category, string Name)> SelectedCategorySections = new();
        private readonly Dictionary<(OptionCategory Category, string Name), RoundedButton> CategorySectionButtons = new Dictionary<(OptionCategory, string), RoundedButton>();
        private readonly List<(OptionCategory Category, string Name, FlexContainer Header, List<OptionsRow> Rows)> SectionStarts = new();

        private FlexContainer MainMenuLayout { get; set; }
        private FlexContainer HeaderMenuLayout { get; set; }
        private FlexContainer MenuColumnsLayout { get; set; }
        private FlexContainer MenuLayout { get; set; }

        // Right content
        private V2FilterSearchTextbox SearchBox;
        private Bindable<string> SearchQuery { get; } = new Bindable<string>(string.Empty);
        private bool IsSearching => !string.IsNullOrWhiteSpace(SearchQuery.Value);
        private V2Dropdown<Guid> PressetDropdown { get; set; }
        private Bindable<Guid> SelectedPresetId { get; } = new Bindable<Guid>(Guid.Empty);
        private IReadOnlyDictionary<Guid, OptionsPreset> LoadedPresets { get; set; } = new Dictionary<Guid, OptionsPreset>();
        private Guid AppliedPresetId { get; set; } = Guid.Empty;
        private FlexContainer MainContentLayout { get; set; }
        private FlexContainer HeaderContentLayout { get; set; }
        private FlexContainer ContentLayout { get; set; }
        private Sprite ContentBackground { get; set; }
        private HorizontalClippingContainer OptionsScrollClip { get; set; }
        private ScrollContainer OptionsScroll { get; set; }
        private FlexContainer OptionsRows { get; set; }

        public OptionsV2Dialog() : this(null, null, false)
        {
        }

        internal OptionsV2Dialog(SkinStoreV2Lease skin, SkinV2Config config, bool previewMode)
            : base(previewMode ? 0 : 0.75f)
        {
            Skin = skin ?? SkinManager.AcquireV2();
            OwnsSkin = skin == null;
            RootConfig = config ?? Skin.Config;
            PreviewMode = previewMode;
            AppliedPresetId = ConfigManager.SelectedOptionsPresetId.Value;
            CreateLayouts();

            CreateContent();
            CreateLeftMenu();
            CreateSectionMenu();
            CreateMainContent();

            if (!PreviewMode)
            {
                ConfigManager.RecentlyChangedOptions.ValueChanged += OnRecentlyChangedOptionsChanged;
                OptionsList.OptionEdited += OnOptionEdited;
                SearchQuery.ValueChanged += OnSearchQueryChanged;
                WindowManager.VirtualScreenSizeChanged += OnVirtualScreenSizeChanged;
            }
            UpdateLayout();

            SelectCategory(OptionCategory.Video);
        }

        public override void CreateContent()
        {
            // Top left content
            CreateTwoSidedSprite(HeaderMenuLayout, Config.Header.FontSize,
                new ScalableVector2(Config.Header.TitleWidth, Config.Layout.HeaderHeight),
                SkinV2Color.Parse(Config.Header.CategoryColor), LocalizationManager.Get("Screen_Options_Title"),
                SkinV2Color.Parse(Config.Header.TextColor),
                new ScalableVector2(Math.Max(1,
                    Config.Layout.SidebarWidth - Config.Header.TitleWidth), Config.Layout.HeaderHeight),
                SkinV2Color.Parse(Config.Header.BackgroundColor),
                LocalizationManager.Get("Screen_Options_TitleDescription"),
                SkinV2Color.Parse(Config.Header.SecondaryTextColor)
            );

            // Top right content
            var searchStyle = new V2FilterFieldStyle
            {
                Height = Config.Layout.HeaderHeight,
                SearchIconSize = Config.Header.FontSize,
                SearchIconInset = Config.Layout.ContentPadding,
                CornerRadius = Config.Layout.CornerRadius,
                BackgroundColor = SkinV2Color.Parse(Config.Header.BackgroundColor),
                TextColor = SkinV2Color.Parse(Config.Header.TextColor),
                PlaceholderColor = SkinV2Color.Parse(Config.Header.CategoryColor),
                CursorColor = SkinV2Color.Parse(Config.Header.TextColor)
            };

            SearchBox = new V2FilterSearchTextbox(SearchQuery,
                LocalizationManager.Get("Screen_Options_Searchforoptions"),
                FontManager.GetWobbleFont(Config.Header.Font), Config.Header.FontSize, searchStyle,
                Config.Header.SearchWidth)
            {
                Parent = HeaderContentLayout
            };
            HeaderContentLayout.SetItemOptions(SearchBox, new FlexItemOptions
            {
                Basis = Config.Header.SearchWidth,
                Shrink = 1
            });

            // Presets
            CreatePresetDropdown(AppliedPresetId);
        }

        private void CreatePresetDropdown(Guid selectedId = default)
        {
            if (PressetDropdown != null)
            {
                PressetDropdown.OptionSelected -= OnPresetSelected;
                PressetDropdown.CloseImmediately();
                PressetDropdown.Destroy();
            }

            var dropdownStyle = new SkinV2DropdownConfig
            {
                Height = Config.Layout.HeaderHeight,
                ItemHeight = Config.Layout.HeaderHeight,
                FontSize = Config.Header.FontSize,
                TriggerColor = Config.Header.BackgroundColor,
                ItemColor = SkinV2DropdownConfig.DefaultItemColor,
                HoverColor = Config.Header.HoverColor,
                SelectedItemColor = Config.Sidebar.SelectedColor,
                TextColor = Config.Header.AccentTextColor,
                IconColor = Config.Header.IconColor,
                CornerRadius = Config.Layout.CornerRadius
            };

            var presets = OptionsPressetStore.LoadAll();
            LoadedPresets = presets.ToDictionary(preset => preset.Id);
            if (selectedId != Guid.Empty && !LoadedPresets.ContainsKey(selectedId))
            {
                selectedId = Guid.Empty;
                SetAppliedPreset(Guid.Empty);
            }

            AppliedPresetHasChanges = AppliedPresetId != Guid.Empty &&
                                      LoadedPresets.TryGetValue(AppliedPresetId, out var appliedPreset) &&
                                      !OptionsList.IsPresetCurrent(appliedPreset);
            SelectedPresetId.Value = selectedId;
            var presetEntries = new List<DropdownEntry<Guid>>
            {
                new DropdownOption<Guid>(Guid.Empty, LocalizationManager.Get("Screen_Selection_None"))
            };
            presetEntries.AddRange(presets.Select(preset =>
            {
                var unsaved = preset.Id == AppliedPresetId && AppliedPresetHasChanges;
                return (DropdownEntry<Guid>) new DropdownOption<Guid>(preset.Id,
                    unsaved ? $"* {preset.Name}" : preset.Name, unsaved ? UnsavedPresetColor : null);
            }));
            presetEntries.Add(new DropdownOption<Guid>(SavePresetEntryId,
                LocalizationManager.Get("Screen_Options_SavePreset"), SavePresetActionColor));
            presetEntries.Add(new DropdownOption<Guid>(ManagePresetEntryId,
                LocalizationManager.Get("Screen_Options_ManagePreset"), ManagePresetActionColor));
            presetEntries.Add(new DropdownOption<Guid>(CreatePresetEntryId,
                LocalizationManager.Get("Screen_Options_CreatePreset"), CreatePresetActionColor));

            PressetDropdown = new V2Dropdown<Guid>(Config.Header.PresetWidth, SelectedPresetId, presetEntries,
                FontManager.GetWobbleFont(Config.Header.Font), dropdownStyle, Container)
            {
                Parent = HeaderContentLayout
            };
            HeaderContentLayout.SetItemOptions(PressetDropdown, new FlexItemOptions
            {
                Basis = Config.Header.PresetWidth,
                Shrink = 0
            });

            PressetDropdown.OptionSelected += OnPresetSelected;
            HeaderContentLayout.RefreshLayout();
        }

        private void CreateLeftMenu()
        {
            var menuBackground = new Sprite
            {
                Parent = MenuLayout,
                Size = MenuLayout.Size,
                Tint = SkinV2Color.Parse(Config.Sidebar.BackgroundColor)
            };
            menuBackground.Image = RoundedRectTextureCache.Get(menuBackground.Width,
                menuBackground.Height, Config.Layout.CornerRadius);
            MenuLayout.SetItemOptions(menuBackground, new FlexItemOptions { Basis = 0, Grow = 1 });

            MenuColumnsLayout = new FlexContainer
            {
                Parent = menuBackground,
                Size = menuBackground.Size,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch,
                ColumnGap = LayoutGap
            };

            IconListContainer = new FlexContainer
            {
                Parent = MenuColumnsLayout,
                Size = new ScalableVector2(Config.Layout.IconRailWidth, MenuLayout.Height)
            };
            MenuColumnsLayout.SetItemOptions(IconListContainer, new FlexItemOptions { Basis = IconListContainer.Width });

            SectionListClip = new HorizontalClippingContainer
            {
                Parent = MenuColumnsLayout,
                Size = new ScalableVector2(1, Math.Max(1f, MenuLayout.Height - LayoutGap * 2))
            };
            MenuColumnsLayout.SetItemOptions(SectionListClip, new FlexItemOptions { Basis = 0, Grow = 1, AlignSelf = FlexAlignSelf.Center });

            SectionListScroll = new ScrollContainer(SectionListClip.Size, new ScalableVector2(1, 1))
            {
                Parent = SectionListClip,
                Tint = Color.Transparent,
                AllowScrollbarDragging = true,
                AllowMiddleMouseDragging = true,
                CapturesMouseWheelInput = true,
                UsePreviousSpriteBatchOptions = true
            };
            SectionListScroll.SpriteBatchOptions.SortMode = SpriteSortMode.Immediate;
            SectionListScroll.Scrollbar.UsePreviousSpriteBatchOptions = true;

            V2Scrollbar.Apply(SectionListScroll);

            SectionListLayout = new FlexContainer
            {
                Size = new ScalableVector2(1, 1),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = LayoutGap
            };
            SectionListScroll.AddContainedDrawable(SectionListLayout);

            IconList = new NineSliceSprite(RoundedRectTextureCache.Get(20f, 20f,
                Config.Layout.CornerRadius), new SliceMargins((int) Config.Layout.CornerRadius))
            {
                Parent = Panel, // Must be drawn after Layout to render on top of it
                Alignment = Alignment.TopLeft,
                Size = IconListContainer.Size,
                Tint = SkinV2Color.Parse(Config.Sidebar.RailColor)
            };

            var buttonHeight = (int) Config.Layout.ContentPadding;
            IconRecentSearch = new RoundedButton
            {
                Parent = IconList,
                Position = new ScalableVector2(Config.Layout.ContentPadding, buttonHeight),
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Config.Sidebar.ButtonSize, Config.Sidebar.ButtonSize),
                CornerRadius = Config.Layout.CornerRadius,
                Tint = SkinV2Color.Parse(Config.Sidebar.ButtonColor),
                SetChildrenAlpha = false
            };
            IconRecentSearchText = CreateSpriteText(IconRecentSearch,
                LocalizationManager.Get("Screen_Options_RecentlyChanged"), Config.Header.FontSize,
                SkinV2Color.Parse(Config.Sidebar.TextColor), Alignment.MidLeft, 0,
                new ScalableVector2(Config.Sidebar.ButtonSize + LayoutGap, 0));
            IconRecentSearch.SetIcon(GetOptionsIcon(10),
                new Vector2(Config.Sidebar.IconSize, Config.Sidebar.IconSize));
            IconRecentSearch.Icon.Alignment = Alignment.MidLeft;
            IconRecentSearch.Icon.X = (Config.Sidebar.ButtonSize - Config.Sidebar.IconSize) / 2f;
            IconRecentSearch.Clicked += (s, e) => SelectCategory(null);

            buttonHeight += (int) IconRecentSearch.Height + (int) LayoutGap;

            var separator = new Sprite
            {
                Parent = IconList,
                Position = new ScalableVector2(Config.Layout.ContentPadding, buttonHeight),
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Config.Sidebar.ButtonSize, 2),
                Tint = SkinV2Color.Parse(Config.Sidebar.SeparatorColor),
                SetChildrenAlpha = false
            };

            buttonHeight += (int) separator.Height + (int) LayoutGap;

            foreach (var category in (OptionCategory[])Enum.GetValues(typeof(OptionCategory)))
            {
                var iconButton = new RoundedButton
                {
                    Parent = IconList,
                    Position = new ScalableVector2(Config.Layout.ContentPadding, buttonHeight),
                    Alignment = Alignment.TopLeft,
                    Size = new ScalableVector2(Config.Sidebar.ButtonSize, Config.Sidebar.ButtonSize),
                    CornerRadius = Config.Layout.CornerRadius,
                    Tint = SkinV2Color.Parse(Config.Sidebar.ButtonColor),
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
                CreateSpriteText(iconButton, LocalizationManager.Get(iconRowInfo.Item2),
                    Config.Header.FontSize, SkinV2Color.Parse(Config.Sidebar.TextColor), Alignment.MidLeft,
                    0, new ScalableVector2(Config.Sidebar.ButtonSize + LayoutGap, 0));
                iconButton.SetIcon(GetOptionsIcon(iconRowInfo.Item1),
                    new Vector2(Config.Sidebar.IconSize, Config.Sidebar.IconSize));
                iconButton.Icon.Alignment = Alignment.MidLeft;
                iconButton.Icon.X = (Config.Sidebar.ButtonSize - Config.Sidebar.IconSize) / 2f;
                iconButton.Clicked += (s, e) => SelectCategory(category);
                CategoryIcons.Add(category, iconButton);

                buttonHeight += (int) (Config.Sidebar.ButtonSize + LayoutGap);
            }

            IconToggle = new RoundedButton
            {
                Parent = IconList,
                Alignment = Alignment.BotLeft,
                Position = new ScalableVector2(Config.Layout.ContentPadding,
                    -Config.Layout.ContentPadding),
                Size = new ScalableVector2(Config.Sidebar.ButtonSize, Config.Sidebar.ButtonSize),
                CornerRadius = Config.Layout.CornerRadius,
                Tint = SkinV2Color.Parse(Config.Sidebar.ButtonColor),
                SetChildrenAlpha = false
            };
            CreateSpriteText(IconToggle, LocalizationManager.Get("Screen_Multiplayer_Close"),
                Config.Header.FontSize, SkinV2Color.Parse(Config.Sidebar.TextColor), Alignment.MidLeft,
                0, new ScalableVector2(Config.Sidebar.ButtonSize + LayoutGap, 0));
            IconToggle.SetIcon(GetOptionsIcon(9),
                new Vector2(Config.Sidebar.IconSize, Config.Sidebar.IconSize));
            IconToggle.Icon.Alignment = Alignment.MidLeft;
            IconToggle.Icon.X = (Config.Sidebar.ButtonSize - Config.Sidebar.IconSize) / 2f;
            IconToggle.Clicked += (s, e) => ExpandIconList(!IsIconListExpanded);
        }

        private void CreateSectionMenu()
        {
            foreach (var button in CategorySectionButtons.Values)
                button.Destroy();
            CategorySectionButtons.Clear();

            SelectedCategorySections.Clear();

            var options = GetVisibleOptions();
            var sections = options.DistinctBy(option => (option.Category, option.SectionName)).ToList();
            foreach (var section in sections)
            {
                var name = LocalizationManager.Get(section.SectionName);

                // In the very rare case where search / recently changed would return multiple section with the same name, also display the category it belongs to
                if (!CurrentCategory.HasValue && sections.Count(other => other.SectionName == section.SectionName) > 1)
                    name = $"{LocalizationManager.Get($"Screen_Options_{section.Category}")} - {name}";

                CreateSectionMenuButton((section.Category, section.SectionName), name);
            }
            UpdateSectionListLayout();
            SectionListScroll.ScrollTo(0, 1);
        }
        private void CreateSectionMenuButton((OptionCategory Category, string Name) section, string label)
        {
            var sectionButton = new RoundedButton
            {
                Parent = SectionListLayout,
                Size = new ScalableVector2(236, Config.Sidebar.ButtonSize),
                Tint = SkinV2Color.Parse(Config.Sidebar.SectionButtonColor),
                CornerRadii = RoundedRectCornerRadii.All(Config.Layout.CornerRadius),
                UsePreviousSpriteBatchOptions = true
            };

            var sectionLabel = new MarqueeSpriteText(FontManager.GetWobbleFont(Config.Header.Font), label,
                Config.Header.FontSize, sectionButton.Width - Config.Layout.ContentPadding * 2)
            {
                Parent = sectionButton,
                Alignment = Alignment.MidLeft,
                X = Config.Layout.ContentPadding,
                UsePreviousSpriteBatchOptions = true
            };

            sectionLabel.TextSprite.Tint = SkinV2Color.Parse(Config.Header.TextColor);
            sectionButton.Hovered += (s, e) => sectionLabel.IsActive = true;
            sectionButton.LeftHover += (s, e) => sectionLabel.IsActive = false;
            SectionListLayout.SetItemOptions(sectionButton, new FlexItemOptions { Basis = sectionButton.Height, Shrink = 0 });
            sectionButton.Clicked += (s, e) => OnSectionButtonClicked(section);

            CategorySectionButtons.Add(section, sectionButton);
        }

        private void CreateMainContent()
        {
            ContentBackground = new Sprite
            {
                Parent = ContentLayout,
                Size = ContentLayout.Size,
                Tint = SkinV2Color.Parse(Config.Content.BackgroundColor)
            };
            ContentLayout.SetItemOptions(ContentBackground, new FlexItemOptions { Basis = 0, Grow = 1 });

            OptionsScrollClip = new HorizontalClippingContainer
            {
                Parent = ContentBackground,
                Position = new ScalableVector2(Config.Layout.ContentPadding,
                    Config.Layout.ContentPadding),
                Size = new ScalableVector2(1, 1)
            };

            OptionsScroll = new ScrollContainer(new ScalableVector2(1, 1), new ScalableVector2(1, 1))
            {
                Parent = OptionsScrollClip,
                Tint = Color.Transparent,
                AllowScrollbarDragging = true,
                AllowMiddleMouseDragging = true,
                EasingType = Easing.OutQuint,
                CapturesMouseWheelInput = true,
                UsePreviousSpriteBatchOptions = true
            };
            OptionsScroll.SpriteBatchOptions.SortMode = SpriteSortMode.Immediate;
            OptionsScroll.Scrollbar.UsePreviousSpriteBatchOptions = true;
            V2Scrollbar.Apply(OptionsScroll);
            SectionListScroll.ScrollSpeed = OptionsScroll.ScrollSpeed;
            SectionListScroll.EasingType = OptionsScroll.EasingType;
            SectionListScroll.TimeToCompleteScroll = OptionsScroll.TimeToCompleteScroll;
            SectionListScroll.TimeToCompleteMiddleMouseScroll = OptionsScroll.TimeToCompleteMiddleMouseScroll;

            OptionsRows = new FlexContainer
            {
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = LayoutGap
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
            UpdateSearchUpdateButton(options.Count);

            foreach (var option in options)
            {
                var control = OptionsList.CreateControl(option, Container);
                if (control == null)
                    continue;

                if (SectionStarts.Count == 0 || SectionStarts[SectionStarts.Count - 1].Category != option.Category || SectionStarts[SectionStarts.Count - 1].Name != option.SectionName)
                {
                    var header = CreateSectionHeader(option.Category, option.SectionName);
                    SectionStarts.Add((option.Category, option.SectionName, header, new List<OptionsRow>()));
                }

                var row = new OptionsRow(option)
                {
                    Parent = OptionsRows,
                    Size = new ScalableVector2(1, 54)
                };
                OptionsRows.SetItemOptions(row, new FlexItemOptions { Basis = row.Height, Shrink = 0 });
                SectionStarts[SectionStarts.Count - 1].Rows.Add(row);
                row.SetControl(control);
                UseScrollSpriteBatch(row);
            }

            UpdateOptionsContentLayout();
            OptionsScroll.ScrollTo(0, 1);
            UpdateSectionHighlight();
        }

        private IReadOnlyList<OptionsDefinition> GetVisibleOptions() => CurrentCategory.HasValue
            ? OptionsList.All.Where(option => option.Category == CurrentCategory.Value).ToList()
            : IsSearching ? OptionsList.Search(SearchQuery.Value) : OptionsList.Recent;

        private void UpdateSearchUpdateButton(int resultCount)
        {
            if (IsSearching)
            {
                IconRecentSearchText.Text = GetSearchResultsText(resultCount);
                IconRecentSearch.SetIcon(GetOptionsIcon(0),
                    new Vector2(Config.Sidebar.IconSize, Config.Sidebar.IconSize));
                UpdateResultsCount(resultCount);
            }
            else
            {
                IconRecentSearchText.Text = LocalizationManager.Get("Screen_Options_RecentlyChanged");
                IconRecentSearch.SetIcon(GetOptionsIcon(10),
                    new Vector2(Config.Sidebar.IconSize, Config.Sidebar.IconSize));
                SearchBox.SetResultsText(string.Empty);
            }
        }

        private FlexContainer CreateSectionHeader(OptionCategory category, string sectionName)
        {
            var header = new FlexContainer
            {
                Parent = OptionsRows,
                Size = new ScalableVector2(1, Config.Layout.HeaderHeight),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch
            };

            header.Width = CreateTwoSidedSprite(header, Config.Header.FontSize,
                new ScalableVector2(1, header.Height),
                SkinV2Color.Parse(Config.Content.CategoryHeaderColor),
                LocalizationManager.Get($"Screen_Options_{category}"),
                SkinV2Color.Parse(Config.Content.TextColor),
                new ScalableVector2(1, header.Height),
                SkinV2Color.Parse(Config.Content.SectionHeaderColor), LocalizationManager.Get(sectionName),
                SkinV2Color.Parse(Config.Content.SecondaryTextColor),
                fitToText: true);
            OptionsRows.SetItemOptions(header, new FlexItemOptions { Basis = header.Height, Shrink = 0, AlignSelf = FlexAlignSelf.FlexStart });
            UseScrollSpriteBatch(header);

            return header;
        }

        private void ExpandIconList(bool expanded)
        {
            IsIconListExpanded = expanded;

            IconList.ClearAnimations();

            IconToggle.SetIcon(GetOptionsIcon(expanded ? 8 : 9),
                new Vector2(Config.Sidebar.IconSize, Config.Sidebar.IconSize));

            var targetWidth = expanded ? MenuLayout.Width + 1 : IconListContainer.Width; // Don't ask why +1, idk why it doesn't grow properly
            IconList.ChangeWidthTo((int)Math.Round(targetWidth),
                expanded ? Easing.OutCubic : Easing.InCubic,
                Config.Layout.AnimationDurationMilliseconds);
            IconList.Children.ForEach(sprite =>
            {
                sprite.ChangeWidthTo((int)Math.Round(targetWidth - Config.Layout.ContentPadding * 2),
                    expanded ? Easing.OutCubic : Easing.InCubic,
                    Config.Layout.AnimationDurationMilliseconds);
                sprite.Children.ForEach(child => {
                    if (child is SpriteTextPlus spriteText)
                    {
                        spriteText.ClearAnimations(AnimationProperty.Alpha);
                        spriteText.FadeTo(expanded ? 1f : 0f,
                            expanded ? Easing.OutCubic : Easing.InCubic,
                            Config.Layout.AnimationDurationMilliseconds);

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
            sprite1.Image = RoundedRectTextureCache.Get(sprite1.Width, sprite1.Height,
                new RoundedRectCornerRadii(Config.Layout.CornerRadius, 0, 0,
                    Config.Layout.CornerRadius));
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
            sprite2.Image = RoundedRectTextureCache.Get(sprite2.Width, sprite2.Height,
                new RoundedRectCornerRadii(0, Config.Layout.CornerRadius,
                    Config.Layout.CornerRadius, 0));
            parentLayout.SetItemOptions(sprite2, new FlexItemOptions { Basis = sprite2.Width, Grow = fitToText ? 0 : 1, Shrink = fitToText ? 0 : 1 });

            return sprite1.Width + sprite2.Width;
        }

        private SpriteTextPlus CreateSpriteText(Sprite sprite, string text, int size, Color color, Alignment alignment = Alignment.MidCenter, float alpha = 1f, ScalableVector2? position = null, WobbleFontStore fontUsed = null)
        {
            return new SpriteTextPlus(fontUsed ?? FontManager.GetWobbleFont(Config.Header.Font), text, size)
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
                Size = new ScalableVector2(Config.Layout.Width, Config.Layout.Height),
                Tint = Color.Transparent
            };
            Panel.Image = RoundedRectTextureCache.Get(Panel.Width, Panel.Height,
                Config.Layout.CornerRadius);

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
                Size = new ScalableVector2(Config.Layout.SidebarWidth, Panel.Height),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = LayoutGap
            };
            Layout.SetItemOptions(MainMenuLayout, new FlexItemOptions { Basis = MainMenuLayout.Width });

            HeaderMenuLayout = new FlexContainer
            {
                Parent = MainMenuLayout,
                Size = new ScalableVector2(MainMenuLayout.Width, Config.Layout.HeaderHeight),
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
                Size = new ScalableVector2(MainContentLayout.Width, Config.Layout.HeaderHeight),
                Direction = FlexDirection.Row,
                JustifyContent = FlexJustifyContent.SpaceBetween,
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
            SetIconColor(CurrentCategory.HasValue ? CategoryIcons[CurrentCategory.Value] : IconRecentSearch,
                SkinV2Color.Parse(Config.Sidebar.ButtonColor),
                SkinV2Color.Parse(Config.Sidebar.TextColor));

            CurrentCategory = category;
            if (category.HasValue && !string.IsNullOrEmpty(SearchQuery.Value))
                SearchQuery.Value = string.Empty;

            SetIconColor(CurrentCategory.HasValue ? CategoryIcons[CurrentCategory.Value] : IconRecentSearch,
                SkinV2Color.Parse(Config.Sidebar.SelectedColor),
                SkinV2Color.Parse(Config.Header.TextColor));

            // Create buttons for that category
            CreateSectionMenu();
            RefreshOptionsRows();

        }
        private void OnSectionButtonClicked((OptionCategory Category, string Name) section)
        {
            var start = SectionStarts.FirstOrDefault(item => item.Category == section.Category && item.Name == section.Name);
            if (start.Header != null)
                OptionsScroll.ScrollTo(-start.Header.Y, 800);
        }

        private void SetSectionHighlights(IEnumerable<(OptionCategory Category, string Name)> sections)
        {
            var selected = sections.ToHashSet();
            if (SelectedCategorySections.SetEquals(selected))
                return;

            SelectedCategorySections.Clear();
            SelectedCategorySections.UnionWith(selected);
            foreach (var button in CategorySectionButtons)
                button.Value.Tint = SkinV2Color.Parse(SelectedCategorySections.Contains(button.Key)
                    ? Config.Sidebar.SelectedColor : Config.Sidebar.SectionButtonColor);
        }

        private void UpdateSectionHighlight()
        {
            if (SectionStarts.Count == 0)
            {
                SetSectionHighlights(Array.Empty<(OptionCategory, string)>());
                return;
            }

            const float visibilityTolerance = 1;
            var visibleTop = -OptionsScroll.ContentContainer.Y - OptionsRows.Y;
            var visibleBottom = visibleTop + OptionsScroll.Height;
            var visibleSections = SectionStarts
                .Where(section => section.Rows.Count > 0 && section.Rows.All(row =>
                    row.Y >= visibleTop - visibilityTolerance &&
                    row.Y + row.Height <= visibleBottom + visibilityTolerance))
                .Select(section => (section.Category, section.Name))
                .ToHashSet();

            if (visibleSections.Count == 0)
            {
                var mostVisibleSection = SectionStarts
                    .OrderByDescending(section => section.Rows.Sum(row => Math.Max(0, Math.Min(row.Y + row.Height, visibleBottom) - Math.Max(row.Y, visibleTop))))
                    .First();

                visibleSections.Add((mostVisibleSection.Category, mostVisibleSection.Name));
            }

            SetSectionHighlights(visibleSections);
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

        private void OnPresetSelected(object sender, DropdownOptionEventArgs<Guid> args)
        {
            if (args.Option.Value == SavePresetEntryId)
            {
                SelectedPresetId.Value = AppliedPresetId;
                SaveAppliedPreset();
                return;
            }

            if (args.Option.Value == CreatePresetEntryId)
            {
                SelectedPresetId.Value = AppliedPresetId;
                SearchBox.Focused = false;
                DialogManager.Show(new OptionsPresetCreateDialog(OnPresetCreated));
                return;
            }

            if (args.Option.Value == ManagePresetEntryId)
            {
                SelectedPresetId.Value = AppliedPresetId;
                if (AppliedPresetId == Guid.Empty || !LoadedPresets.TryGetValue(AppliedPresetId, out var selectedPreset))
                {
                    NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Options_SelectPresetToManage"));
                    return;
                }

                SearchBox.Focused = false;
                DialogManager.Show(new OptionsPresetManageDialog(selectedPreset,
                    OnPresetSaved, OnPresetDeleted));
                return;
            }

            if (args.Option.Value == Guid.Empty)
            {
                SetAppliedPreset(Guid.Empty);
                PresetDirtyCheckPending = true;
                return;
            }

            if (!LoadedPresets.TryGetValue(args.Option.Value, out var preset))
            {
                SelectedPresetId.Value = AppliedPresetId;
                return;
            }

            try
            {
                var invalidIds = OptionsList.ApplyPreset(preset);
                if (invalidIds.Count != 0)
                {
                    Logger.Error($"Could not apply options preset {preset.Id}: {string.Join(", ", invalidIds)}", LogType.Runtime);
                    SelectedPresetId.Value = AppliedPresetId;
                    NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Options_PresetApplyFailed", preset.Name));
                    return;
                }

                SetAppliedPreset(preset.Id);
                PresetDirtyCheckPending = true;
            }
            catch (Exception e)
            {
                Logger.Error($"Could not apply options preset {preset.Id}: {e}", LogType.Runtime);
                SelectedPresetId.Value = AppliedPresetId;
                NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Options_PresetApplyFailed", preset.Name));
            }
        }

        private void SaveAppliedPreset()
        {
            if (AppliedPresetId == Guid.Empty || !LoadedPresets.TryGetValue(AppliedPresetId, out var preset))
            {
                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("Screen_Options_SelectPresetToSave"));
                return;
            }

            OptionsPreset saved;
            try
            {
                saved = OptionsList.CapturePreset(preset.Id, preset.Name);
                OptionsPressetStore.Save(saved);
            }
            catch (Exception e)
            {
                Logger.Error($"Could not save options preset {preset.Name} ({preset.Id}): {e}", LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("Screen_Options_PresetSaveFailed"));
                return;
            }

            OnPresetSaved(saved);
        }

        private void OnPresetCreated(OptionsPreset preset)
        {
            SetAppliedPreset(preset.Id);
            AppliedPresetHasChanges = false;
            CreatePresetDropdown(preset.Id);
        }

        private void OnPresetSaved(OptionsPreset preset)
        {
            SetAppliedPreset(preset.Id);
            AppliedPresetHasChanges = false;
            CreatePresetDropdown(preset.Id);
        }

        private void OnPresetDeleted(Guid id)
        {
            if (AppliedPresetId != id)
                return;

            SetAppliedPreset(Guid.Empty);
            AppliedPresetHasChanges = false;
            CreatePresetDropdown();
        }

        private void SetAppliedPreset(Guid id)
        {
            AppliedPresetId = id;
            ConfigManager.SelectedOptionsPresetId.Value = id;
        }

        private void OnOptionEdited() => PresetDirtyCheckPending = true;

        private void RefreshPresetDirtyState()
        {
            PresetDirtyCheckPending = false;
            var hasChanges = AppliedPresetId != Guid.Empty &&
                             LoadedPresets.TryGetValue(AppliedPresetId, out var preset) &&
                             !OptionsList.IsPresetCurrent(preset);

            if (AppliedPresetHasChanges == hasChanges)
                return;

            AppliedPresetHasChanges = hasChanges;
            CreatePresetDropdown(AppliedPresetId);
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

            UpdateVisibleSectionButtons();
            UpdateVisibleOptionRows();

            if (PresetDirtyCheckPending)
                RefreshPresetDirtyState();

            UpdateSectionHighlight();
        }

        private void UpdateVisibleOptionRows()
        {
            var visibleTop = -OptionsScroll.ContentContainer.Y - OptionsRows.Y;
            var visibleBottom = visibleTop + OptionsScroll.Height;

            foreach (var child in OptionsRows.Children)
                SetScrollableItemVisible(child, child.Y + child.Height >= visibleTop && child.Y <= visibleBottom);
        }

        private void UpdateVisibleSectionButtons()
        {
            var visibleTop = -SectionListScroll.ContentContainer.Y - SectionListLayout.Y;
            var visibleBottom = visibleTop + SectionListScroll.Height;

            foreach (var button in CategorySectionButtons.Values)
                button.Visible = button.Y + button.Height >= visibleTop && button.Y <= visibleBottom;
        }

        private static void SetScrollableItemVisible(Drawable drawable, bool visible)
        {
            if (drawable.Visible == visible)
                return;

            drawable.Visible = visible;

            if (!visible)
                CloseDropdowns(drawable);
        }

        private static void CloseDropdowns(Drawable drawable)
        {
            if (drawable is V2DropdownBase dropdown)
                dropdown.CloseImmediatelyFromRegistry();

            foreach (var child in drawable.Children)
                CloseDropdowns(child);
        }

        private static void UseScrollSpriteBatch(Drawable drawable)
        {
            drawable.UsePreviousSpriteBatchOptions = true;

            // Nested scroll controls (such as SliderV2's value picker textbox) change the
            // graphics-device scissor rectangle before starting their own deferred batch.
            // Reuse the options list's immediate scissor batch so previously drawn rows
            // cannot be flushed against the nested control's much smaller clip rectangle.
            if (drawable is ScrollContainer scrollContainer)
                scrollContainer.SpriteBatchOptions = null;

            foreach (var child in drawable.Children)
                UseScrollSpriteBatch(child);
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

            if (!MouseManager.IsUniquePress(MouseButton.Left))
                return;

            if (NotificationManager.IsPointerOverActiveNotification)
                return;

            if (!Panel.IsHovered())
                DialogManager.Dismiss(this);
        }

        private static bool IsInputRecorderFocused(Drawable drawable) => drawable is InputRecorderV2 { Focused: true } || drawable.Children.Any(IsInputRecorderFocused);

        private static bool IsOptionInputFocused(Drawable drawable) => drawable is InputRecorderV2 { Focused: true } || drawable is Textbox { Focused: true } || drawable.Children.Any(IsOptionInputFocused);

        internal IReadOnlyList<SkinEditorTarget> GetSkinEditorTargets() => new[]
        {
            new SkinEditorTarget("options-layout",
                LocalizationManager.Get("SkinEditor_Component_OptionsLayout"),
                "Screens.Options.Layout", Panel),
            new SkinEditorTarget("options-panel",
                LocalizationManager.Get("SkinEditor_Component_OptionsPanel"),
                "Screens.Options.Panel", Panel),
            new SkinEditorTarget("options-header",
                LocalizationManager.Get("SkinEditor_Component_OptionsHeader"),
                "Screens.Options.Header", HeaderMenuLayout, HeaderContentLayout),
            new SkinEditorTarget("options-sidebar",
                LocalizationManager.Get("SkinEditor_Component_OptionsSidebar"),
                "Screens.Options.Sidebar", IconList, MenuLayout),
            new SkinEditorTarget("options-content",
                LocalizationManager.Get("SkinEditor_Component_OptionsContent"),
                "Screens.Options.Content", ContentBackground)
        };

        internal void RefreshPreviewLayout() => UpdateLayout();

        private void UpdateLayout()
        {
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            Container.Size = Size;

            var navigation = RootConfig.Shared.Navigation;
            var navigationBarHeight = navigation.Button.Size + navigation.EdgePadding * 2;
            var availableHeight = Math.Max(1f, WindowManager.Height - navigationBarHeight * 2);

            Panel.Size = new ScalableVector2(
                Math.Max(1f, Math.Min(Config.Layout.Width, WindowManager.Width - Config.Layout.WindowPadding * 2)),
                Math.Max(1f, Math.Min(Config.Layout.Height, Math.Min(WindowManager.Height - Config.Layout.WindowPadding * 2, availableHeight))));
            Panel.Image = RoundedRectTextureCache.Get(Panel.Width, Panel.Height, Config.Layout.CornerRadius);

            Layout.Size = Panel.Size;

            Layout.RefreshLayout();
            MainMenuLayout.RefreshLayout();
            MainContentLayout.RefreshLayout();
            HeaderMenuLayout.RefreshLayout();
            MenuLayout.RefreshLayout();
            SectionListClip.Height = Math.Max(1f, MenuLayout.Height - LayoutGap * 2);
            MenuColumnsLayout.RefreshLayout();
            SectionListScroll.Size = SectionListClip.Size;
            UpdateSectionListLayout();
            HeaderContentLayout.RefreshLayout();
            ContentLayout.RefreshLayout();
            UpdateOptionsContentLayout();

            SyncIconList();
        }
        private void UpdateSectionListLayout()
        {
            var buttonsHeight = CategorySectionButtons.Count * Config.Sidebar.ButtonSize +
                                Math.Max(0, CategorySectionButtons.Count - 1) * SectionListLayout.RowGap;
            SectionListLayout.Height = Math.Max(1, buttonsHeight);
            SectionListLayout.Width = V2Scrollbar.GetContentWidth(SectionListScroll);
            SectionListScroll.ContentContainer.Height = SectionListLayout.Height;
            SectionListLayout.RefreshLayout();

            foreach (var button in CategorySectionButtons.Values)
            {
                var label = button.Children.OfType<MarqueeSpriteText>().FirstOrDefault();
                if (label == null)
                    continue;

                var labelWidth = Math.Max(1,
                    button.Width - Config.Layout.ContentPadding * 2);
                if (Math.Abs(label.Width - labelWidth) < 0.001f)
                    continue;

                label.Width = labelWidth;
                label.ResetPosition();
            }
        }
        private void UpdateOptionsContentLayout()
        {
            ContentBackground.Image = RoundedRectTextureCache.Get(ContentBackground.Width,
                ContentBackground.Height, Config.Layout.CornerRadius);


            OptionsScrollClip.Size = new ScalableVector2(
                ContentBackground.Width - Config.Layout.ContentPadding,
                ContentBackground.Height - Config.Layout.ContentPadding * 2);
            OptionsScroll.Size = OptionsScrollClip.Size;
            OptionsRows.Width = V2Scrollbar.GetContentWidth(OptionsScroll);

            var rowsHeight = OptionsRows.Children.Sum(row => row.Height) + Math.Max(0, OptionsRows.Children.Count - 1) * OptionsRows.RowGap;
            OptionsRows.Height = Math.Max(
                ContentBackground.Height - Config.Layout.ContentPadding * 2, rowsHeight);
            OptionsScroll.ContentContainer.Height = OptionsRows.Height;
            OptionsRows.RefreshLayout();
            foreach (var section in SectionStarts)
                section.Header.RefreshLayout();
            foreach (var row in OptionsRows.Children.OfType<OptionsRow>())
                row.RefreshLayout();
        }
        private void SyncIconList()
        {
            var offset = IconListContainer.AbsolutePosition - Panel.AbsolutePosition;
            var scale = Panel.AbsoluteScale;
            if (Math.Abs(scale.X) > float.Epsilon)
                offset.X /= scale.X;
            if (Math.Abs(scale.Y) > float.Epsilon)
                offset.Y /= scale.Y;
            var width = IsIconListExpanded ? MenuLayout.Width : IconListContainer.Width;

            IconList.ClearAnimations();
            IconList.Position = new ScalableVector2(offset.X, offset.Y);
            IconList.Size = new ScalableVector2(Math.Max(1f, width), Math.Max(1f, MenuLayout.Height));
        }

        public override void Destroy()
        {
            SearchBox.Focused = false;

            PressetDropdown.CloseImmediately();
            PressetDropdown.OptionSelected -= OnPresetSelected;
            SelectedPresetId.Dispose();

            ConfigManager.RecentlyChangedOptions.ValueChanged -= OnRecentlyChangedOptionsChanged;
            OptionsList.OptionEdited -= OnOptionEdited;
            SearchQuery.ValueChanged -= OnSearchQueryChanged;
            SearchQuery.Dispose();
            WindowManager.VirtualScreenSizeChanged -= OnVirtualScreenSizeChanged;

            base.Destroy();
            if (OwnsSkin)
                Skin.Dispose();
        }
    }

}
