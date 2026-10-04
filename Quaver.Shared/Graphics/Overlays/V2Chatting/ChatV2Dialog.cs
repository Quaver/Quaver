using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Overlays.V2Hub.Users;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Gameplay;
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

namespace Quaver.Shared.Graphics.Overlays.V2Chatting
{
    public class ChatV2Dialog : DialogScreen
    {
        private const float SearchSectionWidth = 480;
        private const float SectionGap = 10;
        private const float SectionPadding = 10;
        private const float HeaderHeight = 40;
        private const float HeaderButtonSize = 40;

        private NineSliceSprite Panel { get; set; }
        private FlexContainer SectionsLayout { get; set; }
        private NineSliceSprite ChatSection { get; set; }
        private NineSliceSprite SearchSection { get; set; }
        private FlexContainer Layout {  get; set; }
        private FlexContainer SearchLayout { get; set; }
        private RoundedButton ChatHeaderBackground { get; set; }
        private RoundedButton SearchHeaderBackground { get; set; }
        private FlexContainer HeaderLayout {  get; set; }
        private HorizontalClippingContainer HeaderTabsClip { get; set; }
        private FlexContainer HeaderTabsLayout { get; set; }
        private Container HiddenTabsContainer { get; set; }
        private FlexContainer SearchHeaderLayout { get; set; }
        private V2FilterSearchTextbox UserSearchBox { get; set; }
        private Bindable<string> UserSearchQuery { get; } = new Bindable<string>(string.Empty);
        private ChatUserSearchList UserSearchList { get; set; }
        private RoundedButton OptionsButton { get; set; }
        private RoundedButton OverflowTabsButton { get; set; }
        private SpriteTextPlus OverflowTabsCount { get; set; }
        private RoundedButton ToggleSearchSectionButton { get; set; }
        private FlexContainer ContentLayout {  get; set; }
        private ChatMessageList MessageList { get; set; }
        private ChatInputBar InputBar { get; set; }
        private UserRightClickOptions UserDropdownMenu { get; set; }
        private ChatChannelOverflowMenu OverflowMenu { get; set; }

        private List<ChatChannel> OverflowChannels { get; } = new List<ChatChannel>();
        private FlexItemOptions OverflowTabsButtonLayoutOptions { get; } = new FlexItemOptions
        {
            Basis = HeaderButtonSize,
            Shrink = 0
        };
        private FlexItemOptions SearchSectionLayoutOptions { get; } = new FlexItemOptions
        {
            Basis = SearchSectionWidth,
            Shrink = 0
        };
        private bool SearchSectionVisible { get; set; } = true;
        private float SearchSectionAnimationProgress { get; set; } = 1;

        private bool IsResizing { get; set; }
        private bool ResizedDuringHeaderPress { get; set; }
        private bool SuppressNextOutsideClick { get; set; }
        private float ResizeStartMouseY { get; set; }
        private float ResizeStartPanelHeight { get; set; }

        Dictionary<ChatChannel, ChatChannelTab> ChatChannels = new Dictionary<ChatChannel, ChatChannelTab>();
        private bool IsClosing { get; set; }
        public ChatV2Dialog() : base(0)
        {
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            Container.Size = Size;
            CreateContent();
            CreateHeaderContent();
            CreateMainContent();

            RefreshDialogLayout();

            ChatSession.JoinedChannels.ItemAdded += OnChannelAdded;
            ChatSession.JoinedChannels.ItemRemoved += OnChannelRemoved;
            ChatSession.ActiveChannel.ValueChanged += OnActiveChannelValueChanged;
            ChatSession.ChannelUpdated += OnChannelUpdated;
            WindowManager.VirtualScreenSizeChanged += OnVirtualScreenSizeChanged;
            Panel.MoveToY(0, Easing.InCubic, 200);
        }

        public override void CreateContent()
        {
            Panel = new NineSliceSprite(RoundedRectTextureCache.Get(20, 20, 6), new SliceMargins(6))
            {
                Parent = Container,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(WindowManager.Width, 620),
                Y = 620,
                Tint = Color.Transparent
            };

            SectionsLayout = new FlexContainer
            {
                Parent = Panel,
                Position = new ScalableVector2(SectionPadding, SectionPadding),
                Size = new ScalableVector2(Panel.Width - SectionPadding * 2,
                    Panel.Height - SectionPadding * 2),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch,
                ColumnGap = SectionGap
            };

            ChatSection = new NineSliceSprite(RoundedRectTextureCache.Get(20, 20, 6), new SliceMargins(6))
            {
                Parent = SectionsLayout,
                Size = new ScalableVector2(1, 1),
                Tint = ColorHelper.FromHex("#273038")
            };
            SectionsLayout.SetItemOptions(ChatSection, new FlexItemOptions { Basis = 0, Grow = 1 });

            SearchSection = new NineSliceSprite(RoundedRectTextureCache.Get(20, 20, 6), new SliceMargins(6))
            {
                Parent = SectionsLayout,
                Size = new ScalableVector2(1, 1),
                Tint = ColorHelper.FromHex("#273038")
            };
            SectionsLayout.SetItemOptions(SearchSection, SearchSectionLayoutOptions);

            Layout = new FlexContainer
            {
                Parent = ChatSection,
                Position = new ScalableVector2(SectionPadding, SectionPadding),
                Size = new ScalableVector2(1, 1),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = SectionGap
            };

            SearchLayout = new FlexContainer
            {
                Parent = SearchSection,
                Position = new ScalableVector2(SectionPadding, SectionPadding),
                Size = new ScalableVector2(1, 1),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = SectionGap
            };

            HiddenTabsContainer = new Container
            {
                Parent = Panel,
                Visible = false,
                Size = new ScalableVector2(1, 1)
            };
        }

        private void CreateHeaderContent()
        {
            ChatHeaderBackground = new RoundedButton
            {
                Parent = ChatSection,
                Size = new ScalableVector2(ChatSection.Width, HeaderHeight + SectionPadding * 2),
                CornerRadii = new RoundedRectCornerRadii(6, 6, 0, 0),
                Tint = ColorHelper.FromHex("#181E25"),
                IsClickable = false,
                IsInteractionEnabled = false
            };

            // Keeps content above the header background
            Layout.Parent = ChatSection;

            HeaderLayout = new FlexContainer
            {
                Parent = Layout,
                Size = new ScalableVector2(Layout.Width, HeaderHeight),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch,
                ColumnGap = SectionGap
            };
            Layout.SetItemOptions(HeaderLayout, new FlexItemOptions { Basis = HeaderHeight, Shrink = 0 });

            HeaderTabsClip = new HorizontalClippingContainer
            {
                Parent = HeaderLayout,
                Size = new ScalableVector2(0, HeaderHeight)
            };
            HeaderLayout.SetItemOptions(HeaderTabsClip, new FlexItemOptions { Basis = 0, Grow = 1 });

            HeaderTabsLayout = new FlexContainer
            {
                Parent = HeaderTabsClip,
                Size = new ScalableVector2(1, HeaderHeight),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch,
                ColumnGap = SectionGap
            };

            OverflowTabsButton = CreateHeaderIconButton(HeaderLayout, GlobalIcon.ChatPanel, (sender, args) => ToggleOverflowMenu());
            OverflowTabsButton.Icon.Size = new ScalableVector2(30, 30);
            OverflowTabsCount = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), string.Empty, 18)
            {
                Parent = OverflowTabsButton,
                Alignment = Alignment.MidCenter,
                X = 10,
                Tint = Color.White,
                Visible = false,
                UsePreviousSpriteBatchOptions = true
            };
            HeaderLayout.SetItemOptions(OverflowTabsButton, OverflowTabsButtonLayoutOptions);

            ToggleSearchSectionButton = CreateHeaderIconButton(HeaderLayout, GlobalIcon.ChannelList, (sender, args) => ToggleSearchSection());
            ToggleSearchSectionButton.Tint = ColorHelper.FromHex("#6B83B2");
            HeaderLayout.SetItemOptions(ToggleSearchSectionButton, new FlexItemOptions { Basis = HeaderButtonSize, Shrink = 0 });

            SearchHeaderBackground = new RoundedButton
            {
                Parent = SearchSection,
                Size = new ScalableVector2(SearchSection.Width, HeaderHeight + SectionPadding * 2),
                CornerRadii = new RoundedRectCornerRadii(6, 6, 0, 0),
                Tint = ColorHelper.FromHex("#181E25"),
                IsClickable = false,
                IsInteractionEnabled = false
            };

            // Keep search components above the header background
            SearchLayout.Parent = SearchSection;

            SearchHeaderLayout = new FlexContainer
            {
                Parent = SearchLayout,
                Size = new ScalableVector2(SearchLayout.Width, HeaderHeight),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch,
                ColumnGap = SectionGap
            };
            SearchLayout.SetItemOptions(SearchHeaderLayout, new FlexItemOptions { Basis = HeaderHeight, Shrink = 0 });

            var searchStyle = new V2FilterFieldStyle
            {
                Height = HeaderHeight,
                SearchIconSize = 18,
                SearchIconInset = 10,
                CornerRadius = SkinV2BorderRadiusConfig.Normal,
                BackgroundColor = ColorHelper.FromHex("#273038"),
                TextColor = Color.White,
                PlaceholderColor = ColorHelper.FromHex("#8CAFEA80"),
                CursorColor = Color.White
            };
            UserSearchBox = new V2FilterSearchTextbox(UserSearchQuery, LocalizationManager.Get("Screen_Hub_SearchUsers"), FontManager.GetWobbleFont(Fonts.InterBold), 18, searchStyle, SearchSectionWidth - SectionPadding * 2 - SectionGap - HeaderButtonSize)
            {
                Parent = SearchHeaderLayout
            };
            UserSearchBox.SetResultsText(string.Empty);
            SearchHeaderLayout.SetItemOptions(UserSearchBox, new FlexItemOptions { Basis = 0, Grow = 1 });

            OptionsButton = new RoundedButton((sender, args) => DialogManager.Show(new ChatSettingsDialog()))
            {
                Parent = SearchHeaderLayout,
                Size = new ScalableVector2(HeaderButtonSize, HeaderButtonSize),
                CornerRadius = SkinV2BorderRadiusConfig.Normal,
                Tint = ColorHelper.FromHex("#273038")
            };
            OptionsButton.SetIcon(GlobalIcons.Get(GlobalIcon.Options), new Vector2(30, 30));
            SearchHeaderLayout.SetItemOptions(OptionsButton, new FlexItemOptions { Basis = HeaderButtonSize, Shrink = 0 });

            ChatSession.JoinedChannels.Value.ForEach(CreateTabButton);
        }

        private static RoundedButton CreateHeaderIconButton(Drawable parent, GlobalIcon icon, EventHandler clickAction)
        {
            var button = new RoundedButton(clickAction)
            {
                Parent = parent,
                Size = new ScalableVector2(HeaderButtonSize, HeaderButtonSize),
                CornerRadius = SkinV2BorderRadiusConfig.Normal,
                Tint = ColorHelper.FromHex("#273038")
            };
            button.SetIcon(GlobalIcons.Get(icon), new Vector2(30, 30));
            return button;
        }

        private void CreateTabButton(ChatChannel channel)
        {
            if (ChatChannels.ContainsKey(channel))
                return;

            var privateUser = channel.IsPrivate ? channel.DirectMessageUser ?? OnlineManager.OnlineUsers.Values.FirstOrDefault(user =>
                user?.OnlineUser != null &&
                string.Equals(user.OnlineUser.Username, channel.Name, StringComparison.OrdinalIgnoreCase)) : null;

            var button = new ChatChannelTab(channel, privateUser, (sender, args) =>
            {
                if (ChatSession.ActiveChannel.Value != channel && !ResizedDuringHeaderPress)
                    ChatSession.ActiveChannel.Value = channel;
            })
            {
                Parent = HiddenTabsContainer,
                Size = new ScalableVector2(180, HeaderHeight)
            };
            button.Width = button.PreferredWidth;

            ChatChannels.Add(channel, button);
            UpdateChannelButtonTints();
            RefreshTabOverflow();
        }

        private void RefreshDialogLayout()
        {
            SectionsLayout.Size = new ScalableVector2(
                Math.Max(1, Panel.Width - SectionPadding * 2),
                Math.Max(1, Panel.Height - SectionPadding * 2));
            SectionsLayout.RefreshLayout();

            Layout.Size = new ScalableVector2(
                Math.Max(1, ChatSection.Width - SectionPadding * 2),
                Math.Max(1, ChatSection.Height - SectionPadding * 2));

            ChatHeaderBackground.Size = new ScalableVector2(ChatSection.Width, HeaderHeight + SectionPadding * 2);

            SearchLayout.Size = new ScalableVector2(
                Math.Max(1, SearchSection.Width - SectionPadding * 2),
                Math.Max(1, SearchSection.Height - SectionPadding * 2));

            SearchHeaderBackground.Size = new ScalableVector2(SearchSection.Width, HeaderHeight + SectionPadding * 2);

            Layout.RefreshLayout();
            SearchLayout.RefreshLayout();
            HeaderLayout.RefreshLayout();
            SearchHeaderLayout.RefreshLayout();
            ContentLayout?.RefreshLayout();

            if (!IsResizing && SearchSectionAnimationProgress >= 0.999f)
                UserSearchList?.RefreshViewport();

            HeaderTabsLayout.Size = HeaderTabsClip.Size;
            RefreshTabOverflow();
        }

        private void RefreshTabOverflow() => RefreshTabOverflow(0);

        private void RefreshTabOverflow(int layoutPass)
        {
            if (HeaderTabsLayout == null || HeaderTabsClip == null || HiddenTabsContainer == null)
                return;

            // Calculate tabs width
            var tabs = ChatChannels.ToList();
            var visibleTabs = new List<KeyValuePair<ChatChannel, ChatChannelTab>>();
            var maxWidth = Math.Max(0, HeaderTabsClip.Width);
            var totalWidth = 0f;

            foreach (var tab in tabs)
            {
                var requiredWidth = tab.Value.PreferredWidth + (visibleTabs.Count == 0 ? 0 : SectionGap);
                if (totalWidth + requiredWidth > maxWidth)
                    break;

                visibleTabs.Add(tab);
                totalWidth += requiredWidth;
            }

            // In case of overflow, keep the selected channel on the visible part of the header
            var activeTab = tabs.FirstOrDefault(tab => tab.Key == ChatSession.ActiveChannel.Value);
            if (activeTab.Key != null && !visibleTabs.Any(tab => tab.Key == activeTab.Key) && activeTab.Value.PreferredWidth <= maxWidth)
            {
                while (visibleTabs.Count > 0)
                {
                    var currentTabWidth = GetTabsWidth(visibleTabs) + SectionGap + activeTab.Value.PreferredWidth;
                    if (currentTabWidth <= maxWidth)
                        break;

                    visibleTabs.RemoveAt(visibleTabs.Count - 1);
                }

                visibleTabs.Add(activeTab);
            }

            var visibleChannels = new HashSet<ChatChannel>(visibleTabs.Select(tab => tab.Key));
            foreach (var tab in tabs)
                tab.Value.Parent = HiddenTabsContainer;

            foreach (var tab in tabs.Where(tab => visibleChannels.Contains(tab.Key)))
            {
                tab.Value.Parent = HeaderTabsLayout;
                HeaderTabsLayout.SetItemOptions(tab.Value, new FlexItemOptions { Basis = tab.Value.PreferredWidth, Shrink = 0 });
            }

            // Add all overflow channels to the button dropdown
            OverflowChannels.Clear();
            OverflowChannels.AddRange(tabs
                .Where(tab => !visibleChannels.Contains(tab.Key))
                .Select(tab => tab.Key));

            if (UpdateOverflowTabsButton() && layoutPass < 3)
            {
                HeaderLayout.RefreshLayout();
                HeaderTabsLayout.Size = HeaderTabsClip.Size;
                RefreshTabOverflow(layoutPass + 1);
                return;
            }

            HeaderTabsLayout.RefreshLayout();

            if (OverflowMenu != null)
                DismissOverflowMenu();
        }

        private static float GetTabsWidth(IReadOnlyCollection<KeyValuePair<ChatChannel, ChatChannelTab>> tabs) =>
            tabs.Sum(tab => tab.Value.PreferredWidth) + Math.Max(0, tabs.Count - 1) * SectionGap;

        private bool UpdateOverflowTabsButton()
        {
            var hasOverflow = OverflowChannels.Count > 0;
            OverflowTabsCount.Text = $"+{OverflowChannels.Count}";
            OverflowTabsCount.Visible = hasOverflow;
            OverflowTabsButton.IsClickable = hasOverflow;
            OverflowTabsButton.IsInteractionEnabled = hasOverflow;
            OverflowTabsButton.Tint = ColorHelper.FromHex(hasOverflow ? "#273038" : "#27303880");

            var targetWidth = HeaderButtonSize;
            if (hasOverflow)
            {
                var contentWidth = OverflowTabsButton.Icon.Width + 5 + OverflowTabsCount.Width;
                var contentLeft = -contentWidth / 2f;
                targetWidth = Math.Max(HeaderButtonSize, contentWidth + SectionPadding * 2);

                OverflowTabsButton.Icon.X = contentLeft + OverflowTabsButton.Icon.Width / 2f;
                OverflowTabsCount.X = contentLeft + OverflowTabsButton.Icon.Width + 5 + OverflowTabsCount.Width / 2f;
            }
            else
                OverflowTabsButton.Icon.X = 0;

            var widthChanged = Math.Abs((OverflowTabsButtonLayoutOptions.Basis ?? HeaderButtonSize) - targetWidth) > 0.001f;
            OverflowTabsButtonLayoutOptions.Basis = targetWidth;
            return widthChanged;
        }

        private void ToggleOverflowMenu()
        {
            if (OverflowMenu != null)
            {
                DismissOverflowMenu();
                return;
            }

            if (OverflowChannels.Count == 0)
                return;

            OverflowMenu = new ChatChannelOverflowMenu(260, OverflowChannels, CreateUserMenuStyle(), Container)
            {
                Parent = Container,
                Position = new ScalableVector2(
                    OverflowTabsButton.ScreenRectangle.Right - Container.ScreenRectangle.Left - 260,
                    OverflowTabsButton.ScreenRectangle.Top - Container.ScreenRectangle.Top),
                Size = new ScalableVector2(260, HeaderHeight)
            };
            OverflowMenu.OptionSelected += OnOverflowChannelSelected;
            OverflowMenu.Open();
        }

        private void OnOverflowChannelSelected(object sender, DropdownOptionEventArgs<ChatChannel> e)
        {
            SuppressNextOutsideClick = true;
            ChatSession.ActiveChannel.Value = e.Option.Value;
            AddScheduledUpdate(DismissOverflowMenu);
        }

        private void DismissOverflowMenu()
        {
            if (OverflowMenu == null)
                return;

            OverflowMenu.OptionSelected -= OnOverflowChannelSelected;
            OverflowMenu.Destroy();
            OverflowMenu = null;
        }

        private void ToggleSearchSection()
        {
            DismissOverflowMenu();
            SearchSectionVisible = !SearchSectionVisible;

            if (SearchSectionVisible && SearchSection.Parent != SectionsLayout)
            {
                SearchSection.Visible = true;
                SearchSection.Parent = SectionsLayout;
                SectionsLayout.SetItemOptions(SearchSection, SearchSectionLayoutOptions);
            }
            ToggleSearchSectionButton.Tint = SearchSectionVisible ? ColorHelper.FromHex("#6B83B2") : ColorHelper.FromHex("#273038");
        }

        private void UpdateSearchSectionAnimation(GameTime gameTime)
        {
            var target = SearchSectionVisible ? 1f : 0f;
            if (Math.Abs(target - SearchSectionAnimationProgress) <= 0.001f)
                return;

            var change = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 200);
            SearchSectionAnimationProgress = target > SearchSectionAnimationProgress
                ? Math.Min(target, SearchSectionAnimationProgress + change)
                : Math.Max(target, SearchSectionAnimationProgress - change);

            var easedProgress = EasingFunctions.Perform(Easing.InOutCubic, 0, 1, SearchSectionAnimationProgress);
            SearchSectionLayoutOptions.Basis = SearchSectionWidth * easedProgress;
            SectionsLayout.ColumnGap = SectionGap * easedProgress;

            if (SearchSectionAnimationProgress <= 0.001f)
            {
                SearchSection.Parent = Panel;
                SearchSection.Visible = false;
            }

            RefreshDialogLayout();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateSearchSectionAnimation(gameTime);
            base.Update(gameTime);
        }

        private void CreateMainContent()
        {
            ContentLayout = new FlexContainer
            {
                Parent = Layout,
                Size = new ScalableVector2(Layout.Width, 0),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = 10
            };
            Layout.SetItemOptions(ContentLayout, new FlexItemOptions { Basis = 0, Grow = 1 });

            MessageList = new ChatMessageList(ChatSession.ActiveChannel, new ScalableVector2(ContentLayout.Width, 0))
            {
                Parent = ContentLayout,
                Tint = ColorHelper.FromHex("#273038")
            };
            MessageList.SenderMenuRequested += ShowUserMenu;
            ContentLayout.SetItemOptions(MessageList, new FlexItemOptions { Basis = 0, Grow = 1 });

            InputBar = new ChatInputBar(ChatSession.ActiveChannel,
                new ScalableVector2(ContentLayout.Width, 40))
            {
                Parent = ContentLayout,
            };
            ContentLayout.SetItemOptions(InputBar, new FlexItemOptions { Basis = 40, Shrink = 0 });

            UserSearchList = new ChatUserSearchList(ChatSession.ActiveChannel, UserSearchQuery,
                new ScalableVector2(SearchLayout.Width, 1))
            {
                Parent = SearchLayout,
                Tint = ColorHelper.FromHex("#273038"),
                EasingType = Easing.OutQuint,
                TimeToCompleteScroll = 1200,
                ScrollSpeed = 220,
                InputEnabled = true
            };
            UserSearchList.Scrollbar.Width = 4;
            UserSearchList.Scrollbar.Tint = ColorHelper.FromHex("#D9E3F4");
            UserSearchList.OnRowClicked += ShowUserMenu;
            SearchLayout.SetItemOptions(UserSearchList, new FlexItemOptions { Basis = 0, Grow = 1 });

        }

        private void UpdateChannelButtonTints()
        {
            foreach (var channels in ChatChannels)
            {
                var isActive = channels.Key == ChatSession.ActiveChannel.Value;
                channels.Value.Tint = isActive ? ColorHelper.FromHex("#6B83B2") : ColorHelper.FromHex("#273038");
                channels.Value.Label.Tint = !isActive && channels.Key.IsUnread ? ColorHelper.FromHex("#FFE032") : Color.White;
            }
        }

        private void OnChannelAdded(object sender, BindableListItemAddedEventArgs<ChatChannel> channel) => AddScheduledUpdate(() =>
        {
            if (!IsDisposed)
                CreateTabButton(channel.Item);
        });

        private void OnChannelRemoved(object sender, BindableListItemRemovedEventArgs<ChatChannel> channel) => AddScheduledUpdate(() => RemoveTabButton(channel.Item));

        private void RemoveTabButton(ChatChannel channel)
        {
            if (IsDisposed || !ChatChannels.Remove(channel, out var button))
                return;

            button.Destroy();
            RefreshTabOverflow();
        }

        private void OnActiveChannelValueChanged(object sender, BindableValueChangedEventArgs<ChatChannel> e) => AddScheduledUpdate(() =>
        {
            if (IsDisposed)
                return;

            UpdateChannelButtonTints();
            RefreshTabOverflow();
            UserSearchList.RefreshResult(e.Value);
        });

        private void OnChannelUpdated(ChatChannel channel) => AddScheduledUpdate(() =>
        {
            if (!IsDisposed && ChatChannels.ContainsKey(channel))
                UpdateChannelButtonTints();
        });

        public override void HandleInput(GameTime gameTime)
        {
            if (HandlePanelResizing())
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Close();

            if (MouseManager.IsUniqueClick(MouseButton.Left))
            {
                if (SuppressNextOutsideClick)
                {
                    SuppressNextOutsideClick = false;
                    return;
                }

                if (!Panel.IsHovered())
                    Close();
            }
        }

        private bool HandlePanelResizing()
        {
            if (!IsResizing && MouseManager.IsUniquePress(MouseButton.Left) && HeaderLayout.IsHovered() &&
                !OverflowTabsButton.IsHovered && !ToggleSearchSectionButton.IsHovered)
            {
                IsResizing = true;
                ResizedDuringHeaderPress = false;
                ResizeStartMouseY = MouseManager.CurrentState.Y;
                ResizeStartPanelHeight = Panel.Height;
                return true;
            }

            if (!IsResizing)
                return false;

            if (MouseManager.CurrentState.LeftButton == ButtonState.Released)
            {
                IsResizing = false;
                ResizedDuringHeaderPress = false;
                UserSearchList?.RefreshViewport();
                return true;
            }

            var mouseDelta = ResizeStartMouseY - MouseManager.CurrentState.Y;
            if (Math.Abs(mouseDelta) > 1)
                ResizedDuringHeaderPress = true;

            SetPanelHeight(ResizeStartPanelHeight + mouseDelta);
            return true;
        }

        private void SetPanelHeight(float height)
        {
            var maximumHeight = Math.Max(1, WindowManager.Height - 10);
            var minimumHeight = Math.Min(240, maximumHeight);
            var clampedHeight = MathHelper.Clamp(height, minimumHeight, maximumHeight);

            if (Math.Abs(Panel.Height - clampedHeight) < 0.001f)
                return;

            Panel.Height = clampedHeight;
            RefreshDialogLayout();
        }

        private void OnVirtualScreenSizeChanged(object sender, WindowVirtualScreenSizeChangedEventArgs e) => AddScheduledUpdate(() =>
        {
            if (IsDisposed)
                return;

            Size = new ScalableVector2(e.Size.X, e.Size.Y);
            Container.Size = Size;
            Panel.Width = e.Size.X;
            Panel.Height = Math.Min(Panel.Height, Math.Max(1, e.Size.Y - SectionGap));
            RefreshDialogLayout();
        });

        private void ShowUserMenu(User user)
        {
            DismissUserMenu();

            UserDropdownMenu = new UserRightClickOptions(user, 200, CreateUserMenuStyle(), Container)
            {
                Parent = Container,
                Position = new ScalableVector2(
                    MouseManager.CurrentState.X - Container.AbsolutePosition.X,
                    MouseManager.CurrentState.Y - Container.AbsolutePosition.Y)
            };
            UserDropdownMenu.OptionSelected += (sender, args) => SuppressNextOutsideClick = true;
            UserDropdownMenu.Open();
        }

        private static SkinV2DropdownConfig CreateUserMenuStyle() => new SkinV2DropdownConfig
        {
            Height = 40,
            ItemHeight = 40,
            FontSize = 18,
            TriggerColor = "#181E25FF",
            ItemColor = "#181E25FF",
            HoverColor = "#354451FF",
            SelectedItemColor = "#6B83B2FF",
            TextColor = "#8CAFEAFF",
            IconColor = "#8CAFEAFF",
            CornerRadius = SkinV2BorderRadiusConfig.Normal
        };

        private void DismissUserMenu()
        {
            UserDropdownMenu?.Destroy();
            UserDropdownMenu = null;
        }

        public void Close()
        {
            if (IsClosing) return;

            IsClosing = true;
            InputBar.Focused = false;
            UserSearchBox.Focused = false;

            ClearAnimations();
            Panel.MoveToY((int)WindowManager.Height, Easing.OutCubic, 200);

            if (GameBase.Game is QuaverGame game && game.CurrentScreen?.Type == QuaverScreenType.Gameplay)
            {
                if (game.CurrentScreen is GameplayScreen gameplay)
                    game.GlobalUserInterface.Cursor.Alpha = gameplay.InReplayMode && gameplay.SpectatorClient == null ? 1 : 0;
            }

            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), 300);
        }

        public override void Destroy()
        {
            ChatSession.JoinedChannels.ItemAdded -= OnChannelAdded;
            ChatSession.JoinedChannels.ItemRemoved -= OnChannelRemoved;
            ChatSession.ActiveChannel.ValueChanged -= OnActiveChannelValueChanged;
            ChatSession.ChannelUpdated -= OnChannelUpdated;
            WindowManager.VirtualScreenSizeChanged -= OnVirtualScreenSizeChanged;
            MessageList.SenderMenuRequested -= ShowUserMenu;
            UserSearchList.OnRowClicked -= ShowUserMenu;

            DismissOverflowMenu();
            DismissUserMenu();
            base.Destroy();
            UserSearchQuery.Dispose();
        }
    }
}
