/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Overlays.Chat.Components;
using Quaver.Shared.Graphics.Overlays.Chat.Components.Channels;
using Quaver.Shared.Graphics.Overlays.Chat.Components.Dialogs;
using Quaver.Shared.Graphics.Overlays.Chat.Components.Messages;
using Quaver.Shared.Graphics.Overlays.Chat.Components.Topic;
using Quaver.Shared.Graphics.Overlays.Chat.Components.Users;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Scheduling;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Overlays.Chat
{
    public class ChatOverlay : DialogScreen
    {
        /// <summary>
        ///     The container for the chat dialog.
        /// </summary>
        public Sprite DialogContainer { get; private set; }

        /// <summary>
        ///     The container for the chat channels.
        /// </summary>
        public Sprite ChannelContainer { get; private set; }

        /// <summary>
        ///     The UI for the chat channel list.
        /// </summary>
        public ChatChannelList ChatChannelList { get; private set; }

        /// <summary>
        ///     The container for all of the chat messages.
        /// </summary>
        public Sprite MessageContainer { get; private set; }

        /// <summary>
        ///     When no channels are available, this message container will be displayed.
        /// </summary>
        public Sprite NoChannelMessageContainer { get; private set; }

        /// <summary>
        ///     The container for the chat textbox.
        /// </summary>
        public Sprite TextboxContainer { get; private set; }

        /// <summary>
        ///     The textbox to send chat messages.
        /// </summary>
        public ChatTextbox ChatTextbox { get; private set; }

        /// <summary>
        ///     The container for the channel topic.
        /// </summary>
        public Sprite ChannelHeaderContainner { get; private set; }

        /// <summary>
        ///     The actual channel header.
        /// </summary>
        public ChannelHeader ChannelHeader { get; private set; }

        /// <summary>
        ///     The container for the current channel.
        /// </summary>
        public Sprite CurrentTopicContainer { get; private set; }

        /// <summary>
        ///     The current
        /// </summary>
        public CurrentTopic CurrentTopic { get; private set; }

        /// <summary>
        ///     The active channel for the overlay.
        /// </summary>
        public ChatChannel ActiveChannel { get; set; }

        /// <summary>
        ///     All of the chat message containers.
        /// </summary>
        public Dictionary<ChatChannel, ChatMessageContainer> ChannelMessageContainers { get; }

        /// <summary>
        ///     If the overlay is actually active.
        /// </summary>
        public static bool IsActive => ChatManager.IsActive;

        /// <summary>
        ///     The top divider line for the dialog.
        /// </summary>
        private Sprite TopDividerLine { get; set; }

        /// <summary>
        ///     The bottom divider line for the dialog.
        /// </summary>
        private Sprite BottomDividerLine { get; set; }

        /// <summary>
        ///     The left side divider line for the dialog.
        /// </summary>
        private Sprite LeftDividerLine { get; set; }

        /// <summary>
        ///     The right side divider line for the dialog.
        /// </summary>
        private Sprite RightDividerLine { get; set; }

        /// <summary>
        ///     The container for the online users header.
        /// </summary>
        public Sprite OnlineUsersHeaderContainer { get; private set; }

        /// <summary>
        ///     The container that holds the online users.
        /// </summary>
        public Sprite OnlineUsersContainer { get; private set; }

        /// <summary>
        ///     The actual header for the online users.
        /// </summary>
        public OnlineUsersHeader OnlineUsersHeader { get; private set; }

        /// <summary>
        ///     The interface for filtering the online users.
        /// </summary>
        public OnlineUserFilters OnlineUserFilters { get; private set; }

        /// <summary>
        ///     The list of online users.
        /// </summary>
        public OnlineUserList OnlineUserList { get; private set; }

        /// <summary>
        ///     The dialog to join chat channels.
        /// </summary>
        public JoinChannelDialog JoinChannelDialog { get; private set; }

        /// <summary>
        ///     The amount of time that has passed since the join channel dialog was last opened.
        /// </summary>
        public double TimeSinceLastChannelDialogOpened { get; private set; } = 1000;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ChatOverlay() : base(0f)
        {
            CreateContent();
            ChannelMessageContainers = new Dictionary<ChatChannel, ChatMessageContainer>();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreateDialogContainer();
            CreateChannelContainer();
            CreateMessageContainer();
            CreateTextboxContainer();
            CreateChannelHeaderContainer();
            CreateCurrentTopicContainer();
            CreateNoChannelMessageContainer();
            CreateOnlineUsersHeaderContainer();
            CreateOnlineUsersContainer();

            ChannelHeader = new ChannelHeader(this);
            ChatTextbox = new ChatTextbox(this);
            ChatChannelList = new ChatChannelList(this);
            CurrentTopic = new CurrentTopic(this);
            OnlineUsersHeader = new OnlineUsersHeader(this);
            OnlineUserFilters = new OnlineUserFilters(this);
            OnlineUserList = new OnlineUserList(this);

            CreateDividerLines();

            DialogContainer.X = -DialogContainer.Width;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            TimeSinceLastChannelDialogOpened += gameTime.ElapsedGameTime.TotalMilliseconds;
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
        }

        /// <inheritdoc />
        ///  <summary>
        ///      This is a hack to prevent the chat dialog from being completely destroyed so, it can be
        ///      reused.
        ///      TODO: Fix this in Wobble.
        ///  </summary>
        public override void Destroy()
        {
        }

        /// <summary>
       ///     Creates the dialog container sprite.
       /// </summary>
        private void CreateDialogContainer() => DialogContainer = new Sprite
        {
            Parent = Container,
            Size = new ScalableVector2(WindowManager.Width + 1, WindowManager.Height),
            Alignment = Alignment.MidLeft,
            Tint = ColorHelper.HexToColor($"#080A0D"),
            Alpha = 0
        };

        /// <summary>
        ///     Creates the chat channel container.
        /// </summary>
        private void CreateChannelContainer() => ChannelContainer = new Sprite()
        {
            Parent = DialogContainer,
            Size = new ScalableVector2(250, DialogContainer.Height - 60),
            Tint = Color.Red,
            Alpha = 0
        };

        /// <summary>
        ///     Creates the message container.
        /// </summary>
        private void CreateMessageContainer() => MessageContainer = new Sprite()
        {
            Parent = DialogContainer,
            Size = new ScalableVector2(DialogContainer.Width - ChannelContainer.Width - ChannelContainer.Width, ChannelContainer.Height),
            Tint = Color.Red,
            X = ChannelContainer.Width,
            Alpha = 0,
            Visible = true
        };

        /// <summary>
        ///     Creates the message container for when there are no more chat message containers.
        /// </summary>
        private void CreateNoChannelMessageContainer()
        {
            NoChannelMessageContainer = new Sprite()
            {
                Parent = DialogContainer,
                Size = new ScalableVector2(MessageContainer.Width, MessageContainer.Height - TextboxContainer.Height + 2),
                Tint = Color.Black,
                X = ChannelContainer.Width,
                Alpha = 0.85f,
                Y = CurrentTopicContainer.Height,
                SetChildrenVisibility = true
            };

            var dividerLine = new Sprite()
            {
                Parent = NoChannelMessageContainer,
                Size = new ScalableVector2(NoChannelMessageContainer.Width, 2),
                Alpha = 0.35f,
                Y = -2
            };

            NoChannelMessageContainer.Visible = false;
        }

        /// <summary>
        ///     Creates the container for the textbox.
        /// </summary>
        private void CreateTextboxContainer() => TextboxContainer = new Sprite()
        {
            Parent = DialogContainer,
            Size = new ScalableVector2(DialogContainer.Width - 1, DialogContainer.Height - ChannelContainer.Height + 2),
            Y = ChannelContainer.Height,
            Tint = Color.Magenta,
            Alpha = 0
        };

        /// <summary>
        ///     Creates the container for the channel topic.
        /// </summary>
        private void CreateChannelHeaderContainer() => ChannelHeaderContainner = new Sprite()
        {
            Parent = ChannelContainer,
            Size = new ScalableVector2(ChannelContainer.Width, 60),
            Tint = Color.Cyan,
            Alpha = 0
        };

        /// <summary>
        ///     Creates the container for the current channel.
        /// </summary>
        private void CreateCurrentTopicContainer() => CurrentTopicContainer = new Sprite()
        {
            Parent = MessageContainer,
            Size = new ScalableVector2(MessageContainer.Width, ChannelHeaderContainner.Height),
            Tint = Color.Yellow,
            Alpha = 0
        };

        /// <summary>
        ///     Creates the container for where the online users (header) will be.
        /// </summary>
        private void CreateOnlineUsersHeaderContainer() => OnlineUsersHeaderContainer = new Sprite()
        {
            Parent = DialogContainer,
            Size = new ScalableVector2(ChannelHeaderContainner.Width, ChannelHeaderContainner.Height),
            X = MessageContainer.X + MessageContainer.Width,
            Alpha = 0f,
        };

        /// <summary>
        ///     Creates the container that holds the actual online users.
        /// </summary>
        private void CreateOnlineUsersContainer()
        {
            OnlineUsersContainer = new Sprite()
            {
                Parent = DialogContainer,
                Size = new ScalableVector2(ChannelContainer.Width, ChannelContainer.Height),
                Alpha = 0f,
                X = OnlineUsersHeaderContainer.X
            };
        }

        /// <summary>
        ///     Creates all of the divider lines for the interface.
        /// </summary>
        private void CreateDividerLines()
        {
            TopDividerLine = new Sprite()
            {
                Parent = ChannelHeader,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(DialogContainer.Width, 2),
                Alpha = 0.35f
            };

            BottomDividerLine = new Sprite()
            {
                Parent = TextboxContainer,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(DialogContainer.Width, 2),
                Alpha = 0.35f
            };

            LeftDividerLine = new Sprite()
            {
                Parent = ChannelHeader,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(2, ChatChannelList.Height + ChannelHeader.Height),
                Alpha = 0.35f
            };

            RightDividerLine = new Sprite()
            {
                Parent = MessageContainer,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(2, ChatChannelList.Height + ChannelHeader.Height),
                Alpha = 0.35f,
            };
        }

        /// <summary>
        ///     Resets the parents of the divider lines to make them appear on top.
        /// </summary>
        public void ReparentDividerLines()
        {
            ChannelHeader.Parent = ChannelContainer;
            CurrentTopic.Parent = CurrentTopicContainer;
            OnlineUsersHeader.Parent = OnlineUsersHeaderContainer;
        }

        /// <summary>
        ///     Properly opens and sets the join channel dialog.
        /// </summary>
        public void OpenJoinChannelDialog()
        {
            if (TimeSinceLastChannelDialogOpened < 500)
                return;

            JoinChannelDialog = new JoinChannelDialog(this);
            DialogManager.Show(JoinChannelDialog);
            TimeSinceLastChannelDialogOpened = 0;
        }

        /// <summary>
        ///     Closes the JoinChannelDialog if it is already open.
        /// </summary>
        public void CloseJoinChannelDialog()
        {
            if (JoinChannelDialog == null)
                return;

            TimeSinceLastChannelDialogOpened = 0;

            JoinChannelDialog.InterfaceContainer.Animations.Clear();
            JoinChannelDialog.InterfaceContainer.Animations.Add(new Animation(AnimationProperty.Y, Easing.OutQuint,
                JoinChannelDialog.InterfaceContainer.Y, JoinChannelDialog.InterfaceContainer.Height, 600));

            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(JoinChannelDialog), 450);
        }
    }
}
