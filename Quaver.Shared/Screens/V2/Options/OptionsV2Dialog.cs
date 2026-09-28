using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;
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
        private FlexContainer SectionListLayout;
        /// <summary>
        /// Component that will actually grow / shrink when clicking on the IconToggle button
        /// </summary>
        private NineSliceSprite IconList;
        private readonly Dictionary<OptionCategory, RoundedButton> CategoryIcons = new Dictionary<OptionCategory, RoundedButton>();
        private RoundedButton IconRecentSearch;
        private RoundedButton IconToggle;
        private bool IsIconListExpanded;

        private OptionCategory? CurrentCategory { get; set; } = null;
        private string SelectedCategorySection { get; set; }
        private List<RoundedButton> CategorySectionButtons { get; set; } = new List<RoundedButton>();

        private FlexContainer MainMenuLayout { get; set; }
        private FlexContainer HeaderMenuLayout { get; set; }
        private FlexContainer MenuColumnsLayout { get; set; }
        private FlexContainer MenuLayout { get; set; }

        // Right content
        private V2FilterSearchTextbox SearchBox;
        private Bindable<string> SearchQuery { get; } = new Bindable<string>(string.Empty);
        private V2Dropdown<string> PressetDropdown { get; set; }
        private FlexContainer MainContentLayout { get; set; }
        private FlexContainer HeaderContentLayout { get; set; }
        private FlexContainer ContentLayout { get; set; }

        public OptionsV2Dialog() : base(0.75f)
        {
            CreateLayouts();

            CreateContent();
            CreateLeftMenu();
            CreateSectionMenu();
            CreateMainContent();

            WindowManager.VirtualScreenSizeChanged += OnVirtualScreenSizeChanged;
            UpdateLayout();

            SelectCategory(OptionCategory.Video);
        }

        public override void CreateContent()
        {
            // Top left content
            var optionTitle = new Sprite
            {
                Parent = HeaderMenuLayout,
                Size = new ScalableVector2(89, 40),
                Tint = ColorHelper.FromHex("#4B5973"),

            };
            CreateSpriteText(optionTitle, LocalizationManager.Get("Screen_Options_Title"), 18, Color.White);
            optionTitle.Image = RoundedRectTextureCache.Get(optionTitle.Width, optionTitle.Height, new RoundedRectCornerRadii(6, 0, 0, 6));
            HeaderMenuLayout.SetItemOptions(optionTitle, new FlexItemOptions { Grow = 1 });


            var optionDescription = new Sprite
            {
                Parent = HeaderMenuLayout,
                Size = new ScalableVector2(229, 40),
                Tint = ColorHelper.FromHex("#181E25")

            };
            CreateSpriteText(optionDescription, LocalizationManager.Get("Screen_Options_TitleDescription"), 18, ColorHelper.FromHex("#D9E3F4"));
            optionDescription.Image = RoundedRectTextureCache.Get(optionDescription.Width, optionDescription.Height, new RoundedRectCornerRadii(0, 6, 6, 0));
            HeaderMenuLayout.SetItemOptions(optionDescription, new FlexItemOptions { Grow = 1 });

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

            SectionListLayout = new FlexContainer
            {
                Parent = MenuColumnsLayout,
                Size = new ScalableVector2(0, Math.Max(1f, MenuLayout.Height - 10)),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = 10
            };
            MenuColumnsLayout.SetItemOptions(SectionListLayout, new FlexItemOptions { Basis = 0, Grow = 1, AlignSelf = FlexAlignSelf.FlexEnd });

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
            CreateSpriteText(IconRecentSearch, LocalizationManager.Get("Screen_Options_RecentlyChanged"), 18, ColorHelper.FromHex("#D0DBED"), Alignment.MidLeft, 0, new ScalableVector2(50, 0));
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
            CategorySectionButtons.ForEach(sprite => sprite.Destroy());
            CategorySectionButtons.Clear();

            var allSectionName = "Screen_Options_SectionAll";
            CreateSectionMenuButton(allSectionName, ColorHelper.FromHex("#6B83B2"));
            SelectedCategorySection = allSectionName;

            var categorySections = OptionsList.All.Where(option => option.Category == (CurrentCategory == null ? OptionCategory.Video : CurrentCategory.Value)).DistinctBy(section => section.SectionName).ToList();
            categorySections.ForEach(section => CreateSectionMenuButton(section.SectionName, ColorHelper.FromHex("#181E25")));
        }
        private void CreateSectionMenuButton(string sectionName, Color color)
        {
            var sectionButton = new RoundedButton
            {
                Parent = SectionListLayout,
                Size = new ScalableVector2(236, 40),
                Tint = color,
                CornerRadii = new RoundedRectCornerRadii(6, 6, 6, 6)
            };

            sectionButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get(sectionName), 18, Color.White);
            sectionButton.Label.Alignment = Alignment.MidLeft;
            sectionButton.Label.X = 10;
            SectionListLayout.SetItemOptions(sectionButton, new FlexItemOptions { AlignSelf = FlexAlignSelf.FlexStart });
            sectionButton.Clicked += (s, e) => OnSectionButtonClicked(sectionButton, sectionName);

            CategorySectionButtons.Add(sectionButton);
        }

        private void CreateMainContent()
        {
            var contentBackground = new Sprite
            {
                Parent = ContentLayout,
                Size = ContentLayout.Size,
                Tint = ColorHelper.FromHex("#273038")
            };
            contentBackground.Image = RoundedRectTextureCache.Get(contentBackground.Width, contentBackground.Height, 6f);
            ContentLayout.SetItemOptions(contentBackground, new FlexItemOptions { Basis = 0, Grow = 1 });
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

        private void CreateSpriteText(Sprite sprite, string text, int size, Color color, Alignment alignment = Alignment.MidCenter, float alpha = 1f, ScalableVector2? position = null, WobbleFontStore fontUsed = null)
        {
            new SpriteTextPlus(fontUsed ?? FontManager.GetWobbleFont(Fonts.InterBold), text, size)
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

        private void UpdateResultsCount(int count) => SearchBox.SetResultsText(LocalizationManager.Get(count > 1 ? "Screen_Options_SearchResults" : "Screen_Options_SearchResult", count));
        private void OnVirtualScreenSizeChanged(object sender, WindowVirtualScreenSizeChangedEventArgs e) => UpdateLayout();

        private void SelectCategory(OptionCategory? category)
        {
            if (CurrentCategory == category)
                return;

            // Update button color
            SetIconColor(CurrentCategory.HasValue ? CategoryIcons[CurrentCategory.Value] : IconRecentSearch, ColorHelper.FromHex("#273038"), ColorHelper.FromHex("#D0DBED"));

            CurrentCategory = category;

            SetIconColor(CurrentCategory.HasValue ? CategoryIcons[CurrentCategory.Value] : IconRecentSearch, ColorHelper.FromHex("#6B83B2"), ColorHelper.FromHex("#FFFFFF"));

            // Create buttons for that category
            CreateSectionMenu();

        }
        private void OnSectionButtonClicked(RoundedButton sender, string sectionName)
        {
            CategorySectionButtons.ForEach(button => button.Tint = ColorHelper.FromHex("#181E25"));
            sender.Tint = ColorHelper.FromHex("#6B83B2");

            SelectedCategorySection = sectionName;
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
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
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
            MenuColumnsLayout.RefreshLayout();
            SectionListLayout.RefreshLayout();
            HeaderContentLayout.RefreshLayout();
            ContentLayout.RefreshLayout();

            SyncIconList();
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

            WindowManager.VirtualScreenSizeChanged -= OnVirtualScreenSizeChanged;

            base.Destroy();
        }
    }

}
