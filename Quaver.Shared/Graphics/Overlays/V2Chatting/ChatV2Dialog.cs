using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MoreLinq;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Overlays.V2Hub.Users;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting
{
    public class ChatV2Dialog : DialogScreen
    {
        private NineSliceSprite Panel { get; set; }
        private FlexContainer Layout {  get; set; }
        private FlexContainer HeaderLayout {  get; set; }
        private FlexContainer HeaderTabsLayout { get; set; }
        private RoundedButton OptionsButton { get; set; }
        private FlexContainer ContentLayout {  get; set; }
        private ChatMessageList MessageList { get; set; }
        private ChatInputBar InputBar { get; set; }
        private UserRightClickOptions UserDropdownMenu { get; set; }

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

            Layout.RefreshLayout();
            HeaderLayout.RefreshLayout();
            HeaderTabsLayout.RefreshLayout();
            ContentLayout.RefreshLayout();

            ChatSession.JoinedChannels.ItemAdded += OnChannelAdded;
            ChatSession.JoinedChannels.ItemRemoved += OnChannelRemoved;
            ChatSession.ActiveChannel.ValueChanged += OnActiveChannelValueChanged;

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
                Tint = ColorHelper.FromHex("#273038")
            };

            Layout = new FlexContainer
            {
                Parent = Panel,
                Position = new ScalableVector2(10, 10),
                Size = new ScalableVector2(Panel.Width - 20, Panel.Height - 20),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch,
                RowGap = 10
            };
        }

        private void CreateHeaderContent()
        {
            HeaderLayout = new FlexContainer
            {
                Parent = Layout,
                Size = new ScalableVector2(Layout.Width, 40),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch,
                ColumnGap = 10
            };
            Layout.SetItemOptions(HeaderLayout, new FlexItemOptions { Basis = 40, Shrink = 0 });

            HeaderTabsLayout = new FlexContainer
            {
                Parent = HeaderLayout,
                Size = new ScalableVector2(0, HeaderLayout.Height),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch,
                ColumnGap = 10
            };
            HeaderLayout.SetItemOptions(HeaderTabsLayout, new FlexItemOptions { Basis = 0, Grow = 1 });

            OptionsButton = new RoundedButton
            {
                Parent = HeaderLayout,
                Size = new ScalableVector2(40, 40),
                CornerRadius = SkinV2BorderRadiusConfig.Normal,
                Tint = ColorHelper.FromHex("#181E25")
            };
            OptionsButton.SetIcon(GlobalIcons.Get(GlobalIcon.Options), new Vector2(24, 24));
            HeaderLayout.SetItemOptions(OptionsButton, new FlexItemOptions { Basis = 40, Shrink = 0 });

            ChatSession.JoinedChannels.Value.ForEach(CreateTabButton);
        }

        private void CreateTabButton(ChatChannel channel)
        {
            if (ChatChannels.ContainsKey(channel))
                return;

            var privateUser = channel.IsPrivate
                ? OnlineManager.OnlineUsers.Values.FirstOrDefault(user =>
                    user?.OnlineUser != null &&
                    string.Equals(user.OnlineUser.Username, channel.Name, StringComparison.OrdinalIgnoreCase))
                : null;

            var button = new ChatChannelTab(channel, privateUser, (sender, args) =>
            {
                if (ChatSession.ActiveChannel.Value != channel && !ResizedDuringHeaderPress)
                    ChatSession.ActiveChannel.Value = channel;
            })
            {
                Parent = HeaderTabsLayout,
                Size = new ScalableVector2(180, 40)
            };
            HeaderTabsLayout.SetItemOptions(button, new FlexItemOptions { Basis = 180, Shrink = 0 });

            ChatChannels.Add(channel, button);
            UpdateChannelButtonTints();
            HeaderTabsLayout.RefreshLayout();
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
        }

        private void UpdateChannelButtonTints()
        {
            foreach (var channels in ChatChannels)
            {
                channels.Value.Tint = channels.Key == ChatSession.ActiveChannel.Value ? ColorHelper.FromHex("#6B83B2") : ColorHelper.FromHex("#181E25");
            }
        }

        private void OnChannelAdded(object sender, BindableListItemAddedEventArgs<ChatChannel> channel) => CreateTabButton(channel.Item);
        private void OnChannelRemoved(object sender, BindableListItemRemovedEventArgs<ChatChannel> channel)
        {
            if (!ChatChannels.Remove(channel.Item, out var button))
                return;

            button.Destroy();
            HeaderTabsLayout.RefreshLayout();
        }
        private void OnActiveChannelValueChanged( object sender, BindableValueChangedEventArgs<ChatChannel> e) => UpdateChannelButtonTints();

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
            if (!IsResizing && MouseManager.IsUniquePress(MouseButton.Left) && HeaderLayout.IsHovered())
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

            Layout.Size = new ScalableVector2(Math.Max(1, Panel.Width - 20), Math.Max(1, Panel.Height - 20));
            Layout.RefreshLayout();
            HeaderLayout.RefreshLayout();
            HeaderTabsLayout.RefreshLayout();
            ContentLayout.RefreshLayout();
        }

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
            MessageList.SenderMenuRequested -= ShowUserMenu;
            DismissUserMenu();
            base.Destroy();
        }
    }
}
