using System;
using System.Collections.Generic;
using System.Threading.Channels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MoreLinq;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Gameplay;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting
{
    public class ChatV2Dialog : DialogScreen
    {
        private Sprite Panel { get; set; }
        private FlexContainer Layout {  get; set; }
        private FlexContainer HeaderLayout {  get; set; }
        private FlexContainer HeaderTabsLayout { get; set; }
        private FlexContainer ContentLayout {  get; set; }
        private ChatMessageList MessageList { get; set; }
        private ChatInputBar InputBar { get; set; }

        Dictionary<ChatChannel, RoundedButton> ChatChannels = new Dictionary<ChatChannel, RoundedButton>();
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
            Panel = new Sprite
            {
                Parent = Container,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(WindowManager.Width, 620),
                Y = 620,
                Tint = ColorHelper.FromHex("#273038")
            };
            Panel.Image = RoundedRectTextureCache.Get(Panel.Width, Panel.Height, 6f);

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

            ChatSession.JoinedChannels.Value.ForEach(CreateTabButton);
        }

        private void CreateTabButton(ChatChannel channel)
        {
            if (ChatChannels.ContainsKey(channel))
                return;

            var button = new RoundedButton((sender, args) => ChatSession.ActiveChannel.Value = channel)
            {
                Parent = HeaderTabsLayout,
                Size = new ScalableVector2(180, 40),
                CornerRadius = 6
            };
            button.SetLabel( FontManager.GetWobbleFont(Fonts.InterBold), channel.GetDisplayedName(), 18, Color.White);
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
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Close();

            if (MouseManager.IsUniqueClick(MouseButton.Left) && !Panel.IsHovered())
                Close();
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
            base.Destroy();
        }
    }
}
