using System;
using System.Collections.Generic;
using System.Drawing;
using Quaver.Graphics.Overlays.Chat.Components.Messages;
using Quaver.Online;
using Quaver.Server.Client.Structures;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Graphics.Overlays.Chat.Components.Channels
{
    public class ChatChannelList : ScrollContainer
    {
        /// <summary>
        ///     Reference to the parent overlay.
        /// </summary>
        public ChatOverlay Overlay { get; }

        /// <summary>
        ///     The list of available chat channel buttons.
        /// </summary>
        public List<ChatChannelListButton> Buttons { get; }

        /// <summary>
        ///     The currently select chat channel button.
        /// </summary>
        public ChatChannelListButton SelectedButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="overlay"></param>
        public ChatChannelList(ChatOverlay overlay) : base(new ScalableVector2(overlay.ChannelContainer.Width,
                overlay.ChannelContainer.Height - overlay.ChannelHeader.Height),
            new ScalableVector2(overlay.ChannelContainer.Width, overlay.ChannelContainer.Height - overlay.ChannelHeader.Height))
        {
            Overlay = overlay;
            Buttons = new List<ChatChannelListButton>();

            Parent = Overlay.ChannelContainer;
            Y = Overlay.ChannelHeaderContainner.Height;

            Tint = Color.Black;
            Alpha = 0.85f;
        }

        /// <summary>
        ///     Initializes the chat channels.
        /// </summary>
        public void InitializeChannel(ChatChannel channel, bool autoSelectChannel = true)
        {
            var button = new ChatChannelListButton(this, channel);

            // Calculate the y position of the channel
            button.Y = (ChatManager.JoinedChatChannels.Count - 1) * button.Height;

            // Automatically select the first channel that comes in.
            Overlay.ChannelMessageContainers.Add(channel, new ChatMessageContainer(Overlay, channel));

            if (autoSelectChannel)
                button.SelectChatChannel();
            else
            {
                // Reslect the current channel
                Overlay.ChannelMessageContainers[channel].Visible = false;
            }

            Buttons.Add(button);
            AddContainedDrawable(button);
        }
    }
}