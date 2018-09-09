using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Server.Client.Structures;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Graphics.Overlays.Chat.Components.Channels
{
    public class ChatChannelListButton : TextButton
    {
        /// <summary>
        ///     Reference to the parent chat channel list.
        /// </summary>
        private ChatChannelList ChannelList { get; }

        /// <summary>
        ///     The channel that this button references.
        /// </summary>
        private ChatChannel Channel { get; }

        /// <summary>
        ///     Determines if this button is actually selected.
        /// </summary>
        public bool IsSelected { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="channelList"></param>
        /// <param name="channel"></param>
        public ChatChannelListButton(ChatChannelList channelList, ChatChannel channel)
            : base(UserInterface.BlankBox, Fonts.Exo2Regular24, channel.Name, 0.55f)
        {
            ChannelList = channelList;
            Channel = channel;

            Size = new ScalableVector2(channelList.ContentContainer.Width, 50);

            Alpha = 0;

            Text.Alignment = Alignment.MidLeft;
            Text.TextAlignment = Alignment.MidLeft;
            Text.X = 10;
            Text.Y -= 2;

            Clicked += (o, e) => SelectChatChannel();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (IsSelected)
            {
                Text.TextColor = Color.White;
                Alpha = 0.25f;
                Tint = Color.White;
            }
            else if (IsHovered)
            {
                Text.TextColor = Color.White;
                Alpha = 0.05f;
                Tint = Color.White;
            }
            else
            {
                Text.TextColor = Color.DarkGray;
                Alpha = 0;
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///    Selects this chat channel.
        /// </summary>
        public void SelectChatChannel()
        {
            if (IsSelected)
                return;

            IsSelected = true;
            ChannelList.SelectedButton = this;
            ChannelList.Overlay.ActiveChannel = Channel;

            // Deselect all other buttons.
            ChannelList.Buttons.ForEach(x =>{ if (x != this) x.Deselect(); });

            // Make sure the correct message container is displayed.
            foreach (var chatMessageContainer in ChannelList.Overlay.ChannelMessageContainers)
                chatMessageContainer.Value.Visible = chatMessageContainer.Key == Channel;

            ChannelList.Overlay.CurrentTopic.UpdateTopicText(Channel);
            Console.WriteLine($"New active chanel: {ChannelList.Overlay.ActiveChannel.Name}");
        }

        /// <summary>
        ///     Deslects the chat channel.
        /// </summary>
        public void Deselect() => IsSelected = false;
    }
}