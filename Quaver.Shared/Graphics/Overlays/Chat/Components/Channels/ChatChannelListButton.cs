/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Graphics.Overlays.Chat.Components.Channels
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
        public ChatChannel Channel { get; }

        /// <summary>
        ///     Determines if this button is actually selected.
        /// </summary>
        public bool IsSelected { get; private set; }

        /// <summary>
        ///     If the chat has messages in it that are unread.
        /// </summary>
        public bool IsUnread { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="channelList"></param>
        /// <param name="channel"></param>
        public ChatChannelListButton(ChatChannelList channelList, ChatChannel channel)
            : base(UserInterface.BlankBox, Fonts.Exo2SemiBold, channel.Name, 13)
        {
            ChannelList = channelList;
            Channel = channel;
            DestroyIfParentIsNull = false;

            Size = new ScalableVector2(channelList.ContentContainer.Width - 2, 50);

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
                Text.Tint = Color.White;
                Alpha = 0.45f;
                Tint = Color.White;
            }
            else if (IsHovered)
            {
                Text.Tint = Color.White;
                Alpha = 0.25f;
                Tint = Color.White;
            }
            else if (IsUnread)
            {
                Text.Tint = Color.Yellow;
                Alpha = 0;
            }
            else
            {
                Text.Tint = Color.DarkGray;
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
            IsUnread = false;
            ChannelList.SelectedButton = this;
            ChannelList.Overlay.ActiveChannel = Channel;

            // Deselect all other buttons.
            ChannelList.Buttons.ForEach(x =>{ if (x != this) x.Deselect(); });

            // Make sure the correct message container is displayed.
            foreach (var chatMessageContainer in ChannelList.Overlay.ChannelMessageContainers)
                chatMessageContainer.Value.Visible = chatMessageContainer.Key == Channel;

            ChannelList.Overlay.CurrentTopic.UpdateTopicText(Channel);
            ChannelList.Overlay.ReparentDividerLines();

            ChannelList.Overlay.NoChannelMessageContainer.Visible = false;
        }

        /// <summary>
        ///     Deslects the chat channel.
        /// </summary>
        public void Deselect() => IsSelected = false;
    }
}
