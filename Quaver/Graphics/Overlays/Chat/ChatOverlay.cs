using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Graphics.Overlays.Chat.Components;
using Quaver.Graphics.Overlays.Chat.Components.Channels;
using Quaver.Graphics.Overlays.Chat.Components.Messages;
using Quaver.Graphics.Overlays.Chat.Components.Topic;
using Quaver.Helpers;
using Quaver.Online;
using Quaver.Server.Client.Structures;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Window;

namespace Quaver.Graphics.Overlays.Chat
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

            ChannelHeader = new ChannelHeader(this);
            ChatTextbox = new ChatTextbox(this);
            ChatChannelList = new ChatChannelList(this);
            CurrentTopic = new CurrentTopic(this);

            DialogContainer.X = -DialogContainer.Width;
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
            Size = new ScalableVector2(949, WindowManager.Height),
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
            Size = new ScalableVector2(230, DialogContainer.Height - 60),
            Tint = Color.Red,
            Alpha = 0
        };

        /// <summary>
        ///     Creates the message container.
        /// </summary>
        private void CreateMessageContainer() => MessageContainer = new Sprite()
        {
            Parent = DialogContainer,
            Size = new ScalableVector2(DialogContainer.Width - ChannelContainer.Width - 1, ChannelContainer.Height),
            Tint = Color.LimeGreen,
            X = ChannelContainer.Width,
            Alpha = 0
        };

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
    }
}