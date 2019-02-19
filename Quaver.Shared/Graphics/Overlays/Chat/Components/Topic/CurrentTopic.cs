/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;

namespace Quaver.Shared.Graphics.Overlays.Chat.Components.Topic
{
    public class CurrentTopic : Sprite
    {
        /// <summary>
        ///     Reference to the parent chat overlay.
        /// </summary>
        private ChatOverlay Overlay { get; }

        /// <summary>
        ///     The name of the channel.
        /// </summary>
        public SpriteText ChannelName { get; }

        /// <summary>
        ///     The description of the channel.
        /// </summary>
        public SpriteText ChannelDescription { get; }

        /// <summary>
        ///     The button to close the chat channel.
        /// </summary>
        public ImageButton CloseChannelButton { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="overlay"></param>
        public CurrentTopic(ChatOverlay overlay)
        {
            Overlay = overlay;
            Parent = overlay.CurrentTopicContainer;
            Size = overlay.CurrentTopicContainer.Size;

            Tint = Color.Black;
            Alpha = 0.85f;

            ChannelName = new SpriteText(Fonts.Exo2Bold, "", 14)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Y = -10
            };

            ChannelDescription = new SpriteText(Fonts.SourceSansProSemiBold, "", 12)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Y = 10
            };

            CloseChannelButton = new BorderedTextButton("Close Channel", Color.Crimson, (sender, args) => CloseActiveChatChannel())
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(150, 40),
                X = -15,
                Text =
                {
                    FontSize = 13,
                    Font = Fonts.SourceSansProSemiBold
                }
            };
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            var targetCloseChanelButtonAlpha = CloseChannelButton.IsHovered ? 1 : 0.75f;
            CloseChannelButton.Alpha = MathHelper.Lerp(CloseChannelButton.Alpha, targetCloseChanelButtonAlpha, (float) Math.Min(dt / 60f, 1));

            base.Update(gameTime);
        }

        /// <summary>
        ///     Updates the topic text & close channel button with a new channel.
        /// </summary>
        /// <param name="channel"></param>
        public void UpdateTopicText(ChatChannel channel)
        {
            ChannelName.Text = channel.Name;
            ChannelName.X = 15;

            ChannelDescription.Text = channel.Description;
            ChannelDescription.X = 15;

            // Make the close channel button both visible and clickable.
            CloseChannelButton.Visible = true;
            CloseChannelButton.IsClickable = true;
        }

        /// <summary>
        ///     Updates the topic text if there are no more channels left.
        /// </summary>
        private void UpdateTopicText()
        {
            ChannelName.Text = "No channels available";
            ChannelName.X = 15;

            ChannelDescription.Text = "Join a channel to start chatting!";
            ChannelDescription.X = 15;

            // Make the close channel button neither visible or clickable since there aren't any channels.
            CloseChannelButton.Visible = false;
            CloseChannelButton.IsClickable = false;
        }

        /// <summary>
        ///     Closes the chat channel.
        /// </summary>
        public void CloseActiveChatChannel()
        {
            var channelButton = Overlay.ChatChannelList.SelectedButton;

            // For public channels, request to leave the chat channel.
            if (!Overlay.ActiveChannel.IsPrivate)
            {
                if (ChatManager.JoinedChatChannels.Any(x => x.Name == Overlay.ActiveChannel.Name))
                    OnlineManager.Client.LeaveChatChannel(Overlay.ActiveChannel);
            }

            var tfX = new Animation(AnimationProperty.X, Easing.Linear, channelButton.X, -(channelButton.Width + 5), 100);
            channelButton.Animations.Add(tfX);

            // Check to see if there is another button before this one.
            var buttonIndex = Overlay.ChatChannelList.Buttons.FindIndex(x => x == channelButton);

            // Remove the button from the list of chat channel buttons.
            Overlay.ChatChannelList.Buttons.Remove(channelButton);

            // Remove the channel from the joined chat channels.
            ChatManager.JoinedChatChannels.Remove(Overlay.ActiveChannel);

            // Remove the chat channel container
            var messageContainer = Overlay.ChannelMessageContainers[Overlay.ActiveChannel];
            Overlay.ChannelMessageContainers.Remove(Overlay.ActiveChannel);

            // Destroy the container instantly.
            messageContainer.Destroy();

            // Make sure the buttons are realigned properly after closing this channel.
            Overlay.ChatChannelList.RealignButtons();

            // Destroy the button after it finishes its Animation.
            ThreadScheduler.RunAfter(() => channelButton.Destroy(), 150);

            // Automatically select the button behind it. if it exists.
            if (buttonIndex != -1 && buttonIndex - 1 >= 0)
                Overlay.ChatChannelList.Buttons[buttonIndex - 1].SelectChatChannel();
            // If theres still more buttons ahead, then we want to switch to the first one.
            // This is to handle the case of if the user closes the first channel in the list.
            else if (Overlay.ChatChannelList.Buttons.Count > 0)
                Overlay.ChatChannelList.Buttons.First().SelectChatChannel();
            // No channels exist anymore.
            else
            {
                Overlay.ChatChannelList.SelectedButton = null;
                Overlay.ActiveChannel = null;

                UpdateTopicText();
                Overlay.NoChannelMessageContainer.Visible = true;
            }

            Logger.Debug($"Closed chat channel: `{channelButton.Channel.Name}`", LogType.Runtime);
        }
    }
}
