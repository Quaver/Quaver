using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.V2Hub;
using Quaver.Shared.Graphics.Overlays.V2Hub.Users;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.V2.SkinEditor;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Screens.V2.UI.Filters;
using Quaver.Shared.Skinning;
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
        private const string DefaultChannelName = "#quaver";

        private static bool ShouldSelectDefaultChannel { get; set; } = true;

        private SkinStoreV2Lease Skin { get; }
        private bool OwnsSkin { get; }
        private bool PreviewMode { get; }
        private SkinV2Config RootConfig { get; }
        private SkinV2ChatConfig Config => RootConfig.Screens.Chat;
        private float SearchSectionWidth => Config.Layout.SearchSectionWidth;
        private float SectionGap => Config.Layout.Gap;
        private float SectionPadding => Config.Layout.Padding;
        private float HeaderHeight => Config.Layout.HeaderHeight;
        private float HeaderButtonSize => Config.Layout.HeaderButtonSize;

        private NineSliceSprite Panel { get; set; }
        private FlexContainer SectionsLayout { get; set; }
        private Container ChatSection { get; set; }
        private Container SearchSection { get; set; }
        private FlexContainer Layout {  get; set; }
        private FlexContainer SearchLayout { get; set; }
        private NineSliceSprite ChatHeaderBackground { get; set; }
        private NineSliceSprite ChatBodyBackground { get; set; }
        private NineSliceSprite SearchHeaderBackground { get; set; }
        private NineSliceSprite SearchBodyBackground { get; set; }
        private FlexContainer HeaderLayout {  get; set; }
        private HorizontalClippingContainer HeaderTabsClip { get; set; }
        private FlexContainer HeaderTabsLayout { get; set; }
        private Container HiddenTabsContainer { get; set; }
        private FlexContainer SearchHeaderLayout { get; set; }
        private V2FilterSearchTextbox UserSearchBox { get; set; }
        private Bindable<string> UserSearchQuery { get; } = new Bindable<string>(string.Empty);
        private ChatUserSearchList UserSearchList { get; set; }
        private RoundedButton OverflowTabsButton { get; set; }
        private SpriteTextPlus OverflowTabsCount { get; set; }
        private RoundedButton ToggleSearchSectionButton { get; set; }
        private FlexContainer ContentLayout {  get; set; }
        private ChatMessageList MessageList { get; set; }
        private ChatInputBar InputBar { get; set; }
        private ScrollContainer PreviewMessageList { get; set; }
        private ChatMessageRow PreviewMessageRow { get; set; }
        private ChatPreviewUserList PreviewUserList { get; set; }
        private Sprite PreviewInputBar { get; set; }
        private RoundedButton PreviewTab { get; set; }
        private UserRightClickOptions UserDropdownMenu { get; set; }
        private ChatChannelOverflowMenu OverflowMenu { get; set; }

        private List<ChatChannel> OverflowChannels { get; } = new List<ChatChannel>();
        private FlexItemOptions OverflowTabsButtonLayoutOptions { get; } = new FlexItemOptions
        {
            Shrink = 0
        };
        private FlexItemOptions SearchSectionLayoutOptions { get; } = new FlexItemOptions
        {
            Shrink = 0
        };
        private bool SearchSectionVisible { get; set; } = true;
        private float SearchSectionAnimationProgress { get; set; } = 1;

        private bool IsResizing { get; set; }
        private bool ResizedDuringHeaderPress { get; set; }
        private float ResizeStartMouseY { get; set; }
        private float ResizeStartPanelHeight { get; set; }

        Dictionary<ChatChannel, ChatChannelTab> ChatChannels = new Dictionary<ChatChannel, ChatChannelTab>();
        private bool IsClosing { get; set; }
        private bool FocusInputWhenChannelAvailable { get; set; }
        public ChatV2Dialog() : this(null, null, false)
        {
        }

        internal ChatV2Dialog(SkinStoreV2Lease skin, SkinV2Config config, bool previewMode)
            : base(0)
        {
            if (!previewMode)
            {
                var hub = DialogManager.Dialogs.OfType<OverlayDialog>().LastOrDefault();
                if (hub != null)
                    DialogManager.Dismiss(hub);
            }

            Skin = skin ?? SkinManager.AcquireV2();
            OwnsSkin = skin == null;
            RootConfig = config ?? Skin.Config;
            PreviewMode = previewMode;
            OverflowTabsButtonLayoutOptions.Basis = HeaderButtonSize;
            SearchSectionLayoutOptions.Basis = SearchSectionWidth;

            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            Container.Size = Size;
            CreateContent();
            CreateHeaderContent();
            CreateMainContent();

            RefreshDialogLayout();

            if (!PreviewMode)
            {
                ChatSession.JoinedChannels.ItemAdded += OnChannelAdded;
                ChatSession.JoinedChannels.ItemRemoved += OnChannelRemoved;
                ChatSession.ActiveChannel.ValueChanged += OnActiveChannelValueChanged;
                ChatSession.ChannelUpdated += OnChannelUpdated;
                WindowManager.VirtualScreenSizeChanged += OnVirtualScreenSizeChanged;
                FocusInputWhenChannelAvailable = true;
                TrySelectDefaultChannel();
                FadeTo(0.75f, Easing.Linear, Config.Layout.AnimationDurationMilliseconds);
                Panel.MoveToY(0, Easing.InCubic, Config.Layout.AnimationDurationMilliseconds);
            }
            else
                Panel.Y = 0;
        }

        public override void CreateContent()
        {
            Panel = new NineSliceSprite(RoundedRectTextureCache.Get(20, 20, Config.Layout.CornerRadius), new SliceMargins((int) Config.Layout.CornerRadius))
            {
                Parent = Container,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(WindowManager.Width, Config.Layout.PanelHeight),
                Y = Config.Layout.PanelHeight,
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

            ChatSection = new Container
            {
                Parent = SectionsLayout,
                Size = new ScalableVector2(1, 1)
            };
            SectionsLayout.SetItemOptions(ChatSection, new FlexItemOptions { Basis = 0, Grow = 1 });

            SearchSection = new Container
            {
                Parent = SectionsLayout,
                Size = new ScalableVector2(1, 1)
            };
            SectionsLayout.SetItemOptions(SearchSection, SearchSectionLayoutOptions);

            Layout = new FlexContainer
            {
                Parent = ChatSection,
                Position = new ScalableVector2(SectionPadding, SectionPadding),
                Size = new ScalableVector2(1, 1),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = SectionGap * 2
            };

            SearchLayout = new FlexContainer
            {
                Parent = SearchSection,
                Position = new ScalableVector2(SectionPadding, SectionPadding),
                Size = new ScalableVector2(1, 1),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = SectionGap * 2
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
            ChatHeaderBackground = CreateSectionBackground(ChatSection, true, SkinV2Color.Parse(Config.Panels.HeaderColor));
            ChatHeaderBackground.Size = new ScalableVector2(ChatSection.Width, HeaderHeight + SectionPadding * 2 + 1);

            ChatBodyBackground = CreateSectionBackground(ChatSection, false, SkinV2Color.Parse(Config.Panels.SectionColor));
            ChatBodyBackground.Position = new ScalableVector2(0, HeaderHeight + SectionPadding * 2);
            ChatBodyBackground.Size = new ScalableVector2(ChatSection.Width, Math.Max(1, ChatSection.Height - HeaderHeight - SectionPadding * 2));

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
            OverflowTabsButton.Icon.Size = new ScalableVector2(Config.Tabs.IconSize, Config.Tabs.IconSize);
            OverflowTabsCount = new SpriteTextPlus(FontManager.GetWobbleFont(Config.Tabs.Font), string.Empty, Config.Tabs.FontSize)
            {
                Parent = OverflowTabsButton,
                Alignment = Alignment.MidCenter,
                X = Config.Layout.Gap,
                Tint = SkinV2Color.Parse(Config.Tabs.TextColor),
                Visible = false,
                UsePreviousSpriteBatchOptions = true
            };
            HeaderLayout.SetItemOptions(OverflowTabsButton, OverflowTabsButtonLayoutOptions);

            ToggleSearchSectionButton = CreateHeaderIconButton(HeaderLayout, GlobalIcon.ChannelList, (sender, args) => ToggleSearchSection());
            ToggleSearchSectionButton.Tint = SkinV2Color.Parse(Config.Tabs.SelectedColor);
            HeaderLayout.SetItemOptions(ToggleSearchSectionButton, new FlexItemOptions { Basis = HeaderButtonSize, Shrink = 0 });

            SearchHeaderBackground = CreateSectionBackground(SearchSection, true, SkinV2Color.Parse(Config.Panels.HeaderColor));
            SearchHeaderBackground.Size = new ScalableVector2(SearchSection.Width, HeaderHeight + SectionPadding * 2 + 1);

            SearchBodyBackground = CreateSectionBackground(SearchSection, false, SkinV2Color.Parse(Config.Panels.SectionColor));
            SearchBodyBackground.Position = new ScalableVector2(0, HeaderHeight + SectionPadding * 2);
            SearchBodyBackground.Size = new ScalableVector2(SearchSection.Width, Math.Max(1, SearchSection.Height - HeaderHeight - SectionPadding * 2));

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
                SearchIconSize = Config.Search.IconSize,
                SearchIconInset = Config.Search.IconInset,
                CornerRadius = Config.Layout.CornerRadius,
                BackgroundColor = SkinV2Color.Parse(Config.Search.BackgroundColor),
                TextColor = SkinV2Color.Parse(Config.Search.TextColor),
                PlaceholderColor = SkinV2Color.Parse(Config.Search.PlaceholderColor),
                CursorColor = SkinV2Color.Parse(Config.Search.TextColor)
            };
            UserSearchBox = new V2FilterSearchTextbox(UserSearchQuery, LocalizationManager.Get("Screen_Hub_SearchUsers"), FontManager.GetWobbleFont(Config.Search.Font), Config.Search.FontSize, searchStyle, SearchSectionWidth - SectionPadding * 2)
            {
                Parent = SearchHeaderLayout
            };
            UserSearchBox.SetResultsText(string.Empty);
            SearchHeaderLayout.SetItemOptions(UserSearchBox, new FlexItemOptions { Basis = 0, Grow = 1 });

            if (PreviewMode)
                CreatePreviewTab();
            else
                ChatSession.JoinedChannels.Value.ForEach(CreateTabButton);
        }

        private NineSliceSprite CreateSectionBackground(Drawable parent, bool roundTop, Color tint)
        {
            var radius = Math.Max(0, Config.Layout.CornerRadius);
            var margin = (int) Math.Ceiling(radius);
            var textureSize = Math.Max(20, margin * 2 + 2);
            var radii = roundTop
                ? new RoundedRectCornerRadii(radius, radius, 0, 0)
                : new RoundedRectCornerRadii(0, 0, radius, radius);

            return new NineSliceSprite(RoundedRectTextureCache.Get(textureSize, textureSize, radii), new SliceMargins(margin))
            {
                Parent = parent,
                Tint = tint
            };
        }

        private RoundedButton CreateHeaderIconButton(Drawable parent, GlobalIcon icon, EventHandler clickAction)
        {
            var button = new RoundedButton(clickAction)
            {
                Parent = parent,
                Size = new ScalableVector2(HeaderButtonSize, HeaderButtonSize),
                CornerRadius = Config.Layout.CornerRadius,
                Tint = SkinV2Color.Parse(Config.Tabs.UnselectedColor)
            };
            button.SetIcon(GlobalIcons.Get(icon), new Vector2(Config.Tabs.IconSize, Config.Tabs.IconSize));
            return button;
        }

        private void CreatePreviewTab()
        {
            PreviewTab = new RoundedButton
            {
                Parent = HeaderTabsLayout,
                Size = new ScalableVector2(180, HeaderHeight),
                CornerRadius = Config.Layout.CornerRadius,
                Tint = SkinV2Color.Parse(Config.Tabs.SelectedColor),
                IsClickable = false,
                IsInteractionEnabled = false
            };
            PreviewTab.SetLabel(FontManager.GetWobbleFont(Config.Tabs.Font), "#quaver", Config.Tabs.FontSize, SkinV2Color.Parse(Config.Tabs.TextColor));
            PreviewTab.Label.Alignment = Alignment.MidLeft;
            PreviewTab.Label.X = SectionPadding;
            HeaderTabsLayout.SetItemOptions(PreviewTab, new FlexItemOptions { Basis = PreviewTab.Width, Shrink = 0 });
        }

        private void CreateTabButton(ChatChannel channel)
        {
            if (ChatChannels.ContainsKey(channel))
                return;

            var privateUser = channel.IsPrivate ? channel.DirectMessageUser ?? OnlineManager.OnlineUsers.Values.FirstOrDefault(user =>
                user?.OnlineUser != null &&
                string.Equals(user.OnlineUser.Username, channel.Name, StringComparison.OrdinalIgnoreCase)) : null;

            var button = new ChatChannelTab(channel, privateUser, Config.Tabs, Config.Layout.CornerRadius, (sender, args) =>
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

            var bodyTop = HeaderHeight + SectionPadding * 2;
            ChatHeaderBackground.Size = new ScalableVector2(ChatSection.Width, bodyTop + 1);
            ChatBodyBackground.Position = new ScalableVector2(0, bodyTop);
            ChatBodyBackground.Size = new ScalableVector2(ChatSection.Width, Math.Max(1, ChatSection.Height - bodyTop));

            SearchLayout.Size = new ScalableVector2(
                Math.Max(1, SearchSection.Width - SectionPadding * 2),
                Math.Max(1, SearchSection.Height - SectionPadding * 2));

            SearchHeaderBackground.Size = new ScalableVector2(SearchSection.Width, bodyTop + 1);
            SearchBodyBackground.Position = new ScalableVector2(0, bodyTop);
            SearchBodyBackground.Size = new ScalableVector2(SearchSection.Width, Math.Max(1, SearchSection.Height - bodyTop));

            Layout.RefreshLayout();
            SearchLayout.RefreshLayout();
            HeaderLayout.RefreshLayout();
            SearchHeaderLayout.RefreshLayout();
            ContentLayout?.RefreshLayout();

            if (PreviewMessageList != null)
            {
                PreviewMessageList.ContentContainer.Width = PreviewMessageList.Width;
                PreviewMessageRow?.UpdateContent(PreviewMessageRow.Item, PreviewMessageList.Width);
                PreviewMessageList.ContentContainer.Height = Math.Max(PreviewMessageList.Height, PreviewMessageRow?.Height ?? 1);
            }

            // The section layouts provide the left inset. Let the scrolling lists extend through the
            // matching right inset because ScrollContainer already owns the scrollbar's right gap.
            if (MessageList != null)
                MessageList.Width = Math.Max(1, ContentLayout.Width + SectionPadding);

            if (UserSearchList != null)
                UserSearchList.Width = Math.Max(1, SearchLayout.Width + SectionPadding);

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

        private float GetTabsWidth(IReadOnlyCollection<KeyValuePair<ChatChannel, ChatChannelTab>> tabs) =>
            tabs.Sum(tab => tab.Value.PreferredWidth) + Math.Max(0, tabs.Count - 1) * SectionGap;

        private bool UpdateOverflowTabsButton()
        {
            var hasOverflow = OverflowChannels.Count > 0;
            OverflowTabsCount.Text = $"+{OverflowChannels.Count}";
            OverflowTabsCount.Visible = hasOverflow;
            OverflowTabsButton.IsClickable = hasOverflow;
            OverflowTabsButton.IsInteractionEnabled = hasOverflow;
            var overflowColor = SkinV2Color.Parse(Config.Tabs.UnselectedColor);
            OverflowTabsButton.Tint = hasOverflow ? overflowColor : overflowColor * 0.5f;

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
            ToggleSearchSectionButton.Tint = SkinV2Color.Parse(SearchSectionVisible ? Config.Tabs.SelectedColor : Config.Tabs.UnselectedColor);
        }

        private void UpdateSearchSectionAnimation(GameTime gameTime)
        {
            var target = SearchSectionVisible ? 1f : 0f;
            if (Math.Abs(target - SearchSectionAnimationProgress) <= 0.001f)
                return;

            var change = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / Config.Layout.AnimationDurationMilliseconds);
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
            FocusInputIfRequested();
        }

        private void CreateMainContent()
        {
            ContentLayout = new FlexContainer
            {
                Parent = Layout,
                Size = new ScalableVector2(Layout.Width, 0),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = SectionGap
            };
            Layout.SetItemOptions(ContentLayout, new FlexItemOptions { Basis = 0, Grow = 1 });

            if (PreviewMode)
            {
                CreatePreviewMainContent();
                return;
            }

            MessageList = new ChatMessageList(ChatSession.ActiveChannel, new ScalableVector2(ContentLayout.Width, 0))
            {
                Parent = ContentLayout,
                Tint = SkinV2Color.Parse(Config.Panels.SectionColor)
            };
            MessageList.SenderMenuRequested += ShowUserMenu;
            ContentLayout.SetItemOptions(MessageList, new FlexItemOptions
            {
                Basis = 0,
                Grow = 1,
                AlignSelf = FlexAlignSelf.FlexStart
            });

            InputBar = new ChatInputBar(ChatSession.ActiveChannel, new ScalableVector2(ContentLayout.Width, Config.Layout.InputHeight), Config.Input)
            {
                Parent = ContentLayout,
            };
            ContentLayout.SetItemOptions(InputBar, new FlexItemOptions { Basis = Config.Layout.InputHeight, Shrink = 0 });

            UserSearchList = new ChatUserSearchList(ChatSession.ActiveChannel, UserSearchQuery,
                new ScalableVector2(SearchLayout.Width, 1))
            {
                Parent = SearchLayout,
                Tint = SkinV2Color.Parse(Config.Panels.SectionColor),
                EasingType = Easing.OutQuint,
                TimeToCompleteScroll = 1200,
                ScrollSpeed = 220,
                InputEnabled = true
            };
            UserSearchList.OnRowClicked += ShowUserMenu;
            SearchLayout.SetItemOptions(UserSearchList, new FlexItemOptions
            {
                Basis = 0,
                Grow = 1,
                AlignSelf = FlexAlignSelf.FlexStart
            });
        }

        private void CreatePreviewMainContent()
        {
            var previewUsers = CreatePreviewUsers();
            var quaverBot = previewUsers[0];

            PreviewMessageList = new ScrollContainer(new ScalableVector2(1, 1), new ScalableVector2(1, 1))
            {
                Parent = ContentLayout,
                Tint = SkinV2Color.Parse(Config.Panels.SectionColor)
            };
            ContentLayout.SetItemOptions(PreviewMessageList, new FlexItemOptions { Basis = 0, Grow = 1 });

            var previewMessage = new ChatMessage(DefaultChannelName, LocalizationManager.Get("SkinEditor_ChatPreviewMessage"))
            {
                SenderId = quaverBot.OnlineUser.Id,
                SenderName = quaverBot.OnlineUser.Username,
                Sender = quaverBot,
                Time = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };
            PreviewMessageRow = new ChatMessageRow(previewMessage, 1, PreviewMessageList);
            PreviewMessageList.AddContainedDrawable(PreviewMessageRow);

            PreviewInputBar = new Sprite
            {
                Parent = ContentLayout,
                Size = new ScalableVector2(1, Config.Layout.InputHeight),
                Tint = SkinV2Color.Parse(Config.Input.BackgroundColor)
            };
            ContentLayout.SetItemOptions(PreviewInputBar, new FlexItemOptions { Basis = Config.Layout.InputHeight, Shrink = 0 });
            
            new SpriteTextPlus(FontManager.GetWobbleFont(Config.Input.Font), LocalizationManager.Get("Chat_SendAMessage"), Config.Input.FontSize)
            {
                Parent = PreviewInputBar,
                Alignment = Alignment.MidLeft,
                X = SectionPadding,
                Tint = SkinV2Color.Parse(Config.Search.PlaceholderColor)
            };

            PreviewUserList = new ChatPreviewUserList(previewUsers, new ScalableVector2(1, 1))
            {
                Parent = SearchLayout,
                Tint = SkinV2Color.Parse(Config.Panels.SectionColor)
            };
            SearchLayout.SetItemOptions(PreviewUserList, new FlexItemOptions { Basis = 0, Grow = 1 });
        }

        private static List<User> CreatePreviewUsers() => new()
        {
            CreatePreviewUser(2, "QuaverBot", "US", UserGroups.Bot),
            CreatePreviewUser(3, "Swan", "AU", UserGroups.Swan),
            CreatePreviewUser(4, "Player", "FR", UserGroups.Donator)
        };

        private static User CreatePreviewUser(int id, string username, string country, UserGroups userGroups) =>
            new(new OnlineUser
            {
                Id = id,
                Username = username,
                CountryFlag = country,
                UserGroups = userGroups
            });

        private void UpdateChannelButtonTints()
        {
            foreach (var channels in ChatChannels)
            {
                var isActive = channels.Key == ChatSession.ActiveChannel.Value;
                channels.Value.Tint = SkinV2Color.Parse(isActive ? Config.Tabs.SelectedColor : Config.Tabs.UnselectedColor);
                channels.Value.Label.Tint = SkinV2Color.Parse(!isActive && channels.Key.IsUnread ? Config.Tabs.UnreadTextColor : Config.Tabs.TextColor);
            }
        }

        private void OnChannelAdded(object sender, BindableListItemAddedEventArgs<ChatChannel> channel) => AddScheduledUpdate(() =>
        {
            if (IsDisposed)
                return;

            CreateTabButton(channel.Item);
            TrySelectDefaultChannel();
        });

        private void TrySelectDefaultChannel()
        {
            if (!ShouldSelectDefaultChannel)
                return;

            var channel = ChatSession.JoinedChannels.Value.FirstOrDefault(x =>
                string.Equals(x.Name, DefaultChannelName, StringComparison.OrdinalIgnoreCase));
            if (channel == null)
                return;

            ShouldSelectDefaultChannel = false;
            ChatSession.ActiveChannel.Value = channel;
        }

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

        private void FocusInputIfRequested()
        {
            if (!FocusInputWhenChannelAvailable || ChatSession.ActiveChannel.Value == null)
                return;

            InputBar.Focused = true;
            FocusInputWhenChannelAvailable = false;
        }

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
            {
                Close();
                return;
            }

            if (MouseManager.IsUniquePress(MouseButton.Left))
            {
                if (NotificationManager.IsPointerOverActiveNotification)
                    return;

                if (!Panel.IsHovered() && UserDropdownMenu?.IsHovered() != true && OverflowMenu?.IsHovered() != true)
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

                MessageList?.ScrollToBottom(200);
                UserSearchList?.RefreshViewport(true);
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
            var clampedHeight = ClampPanelHeight(height);

            if (Math.Abs(Panel.Height - clampedHeight) < 0.001f)
                return;

            Panel.Height = clampedHeight;
            RefreshDialogLayout();
        }

        private float ClampPanelHeight(float height)
        {
            var maximumHeight = GetMaximumPanelHeight(WindowManager.Height);
            var minimumHeight = Math.Min(240, maximumHeight);
            return MathHelper.Clamp(height, minimumHeight, maximumHeight);
        }

        private float GetMaximumPanelHeight(float windowHeight) =>
            Math.Max(1, windowHeight - ScreenNavigation.TopSafetyGap + SectionPadding);

        private void OnVirtualScreenSizeChanged(object sender, WindowVirtualScreenSizeChangedEventArgs e) => AddScheduledUpdate(() =>
        {
            if (IsDisposed)
                return;

            Size = new ScalableVector2(e.Size.X, e.Size.Y);
            Container.Size = Size;
            Panel.Width = e.Size.X;
            Panel.Height = Math.Min(Panel.Height, GetMaximumPanelHeight(e.Size.Y));
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
            UserDropdownMenu.Open();
        }

        private static SkinV2DropdownConfig CreateUserMenuStyle() => new SkinV2DropdownConfig
        {
            Height = 40,
            ItemHeight = 40,
            FontSize = 18,
            TriggerColor = "#181E25FF",
            ItemColor = SkinV2DropdownConfig.DefaultItemColor,
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

        internal IReadOnlyList<SkinEditorTarget> GetSkinEditorTargets() => new[]
        {
            new SkinEditorTarget("chat-layout", LocalizationManager.Get("SkinEditor_Component_ChatLayout"), "Screens.Chat.Layout", Panel),
            new SkinEditorTarget("chat-panels", LocalizationManager.Get("SkinEditor_Component_ChatPanels"), "Screens.Chat.Panels", ChatSection, SearchSection),
            new SkinEditorTarget("chat-tabs", LocalizationManager.Get("SkinEditor_Component_ChatTabs"), "Screens.Chat.Tabs", (Drawable) PreviewTab ?? HeaderTabsLayout, OverflowTabsButton, ToggleSearchSectionButton),
            new SkinEditorTarget("chat-search", LocalizationManager.Get("SkinEditor_Component_ChatSearch"), "Screens.Chat.Search", UserSearchBox, (Drawable) PreviewUserList ?? UserSearchList),
            new SkinEditorTarget("chat-input", LocalizationManager.Get("SkinEditor_Component_ChatInput"), "Screens.Chat.Input", (Drawable) PreviewInputBar ?? InputBar)
        };

        internal void RefreshPreviewLayout()
        {
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            Container.Size = Size;
            Panel.Width = WindowManager.Width;
            Panel.Height = Math.Min(Config.Layout.PanelHeight, GetMaximumPanelHeight(WindowManager.Height));
            RefreshDialogLayout();
        }

        public void Close()
        {
            if (IsClosing) return;

            IsClosing = true;
            InputBar.Focused = false;
            UserSearchBox.Focused = false;

            ClearAnimations();
            FadeTo(0, Easing.Linear, Config.Layout.AnimationDurationMilliseconds);
            Panel.MoveToY((int)WindowManager.Height, Easing.OutCubic, Config.Layout.AnimationDurationMilliseconds);

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
            if (MessageList != null)
                MessageList.SenderMenuRequested -= ShowUserMenu;
            if (UserSearchList != null)
                UserSearchList.OnRowClicked -= ShowUserMenu;

            DismissOverflowMenu();
            DismissUserMenu();
            base.Destroy();
            UserSearchQuery.Dispose();
            if (OwnsSkin)
                Skin.Dispose();
        }
    }
}
