using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Graphics.Notifications;
using Quaver.Graphics.Overlays.Chat;
using Quaver.Logging;
using Quaver.Scheduling;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Online.Chat
{
    public static class ChatManager
    {
        /// <summary>
        ///     The overlay for the chat.
        /// </summary>
        public static ChatOverlay Dialog { get; } = new ChatOverlay();

        /// <summary>
        ///     The list of available public chat channels.
        /// </summary>
        public static List<ChatChannel> AvailableChatChannels { get; } = new List<ChatChannel>();

        /// <summary>
        ///     The list of chat channels the user is currently in.
        /// </summary>
        public static List<ChatChannel> JoinedChatChannels { get; } = new List<ChatChannel>();

        /// <summary>
        ///    Determines if the chat overlay is active or not.
        /// </summary>
        public static bool IsActive { get; private set; }

        /// <summary>
        ///     The amount of time that has passed since the overlay was last activated.
        /// </summary>
        private static double TimeSinceLastActivated { get; set; }

        /// <summary>
        ///     Handles global input for the chat overlay
        /// </summary>
        /// <param name="gameTime"></param>
        public static void HandleInput(GameTime gameTime)
        {
            TimeSinceLastActivated += gameTime.ElapsedGameTime.TotalMilliseconds;

            // Only allow the box to be typed into if the overlay is active.
            Dialog.ChatTextbox.Textbox.AlwaysFocused = ChatOverlay.IsActive;
            Dialog.ChatTextbox.Textbox.Focused = ChatOverlay.IsActive;

            if (OnlineManager.Connected && KeyboardManager.IsUniqueKeyPress(Keys.F8) && TimeSinceLastActivated >= 450)
            {
                TimeSinceLastActivated = 0;
                IsActive = !IsActive;

                var targetX = IsActive ?  0 : -Dialog.DialogContainer.Width;

                Dialog.DialogContainer.Transformations.Clear();

                Dialog.DialogContainer.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutQuint,
                    Dialog.DialogContainer.X, targetX, 600));

                if (!IsActive)
                {
                    Dialog.IsClickable = false;
                    Scheduler.RunAfter(() => { DialogManager.Dismiss(Dialog); }, 450);
                }
                else
                {
                    Dialog.IsClickable = true;
                    DialogManager.Show(Dialog);
                }
            }
        }

        /// <summary>
        ///     Sends a chat message to the server.
        /// </summary>
        public static void SendMessage(ChatChannel chan, ChatMessage message)
        {
            // Add the message to the chat channel.
            // Find the channel the message is for.
            var channel = JoinedChatChannels.Find(x => x.Name == chan.Name);

            // Make sure the sender is actual valid.
            message.Sender = OnlineManager.Self;

            // Handle forward slash commands (client sided) and not send to the server.
            if (message.Message.StartsWith("/"))
            {
                HandleClientSideCommands(message);
                return;
            }

            // Check if the sender is muted.
            if (OnlineManager.Self.IsMuted)
            {
                QuaverBot.SendMessage(chan, $"Whoa there! Unfortunately you're muted for another: {OnlineManager.Self.GetMuteTimeLeftString()}.\n" +
                                            $"You won't be able to speak 'till then. Check your profile for more details.");
                return;
            }

            // Add the message to the appropriate channel.
            channel.Messages.Add(message);

            // Add the message to the container.
            Dialog.ChannelMessageContainers[channel].AddMessage(channel, message);

            // Send the message to the server.
            OnlineManager.Client.SendMessage(chan.Name, message.Message);

            // If the channel is private, check if the message receiver is muted.
            if (!chan.IsPrivate)
                return;

            // Get the user if they're online.
            var receiver = OnlineManager.OnlineUsers.Values.ToList().Find(x => x.Username == chan.Name);

            // Don't bother sending the message if they're not online.
            if (receiver == null)
            {
                QuaverBot.SendMessage(chan, "This user is not online.");
                return;
            }

            // If the user is muted, then let the user know this.
            if (receiver.IsMuted)
                QuaverBot.SendMessage(chan, $"Oops! {receiver.Username} is muted for another: {receiver.GetMuteTimeLeftString()}.");
        }

         /// <summary>
        ///     Called when we received an available chat channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnAvailableChatChannel(object sender, AvailableChatChannelEventArgs e)
        {
            AvailableChatChannels.Add(e.Channel);
            Logger.LogImportant($"Received new available chat channel: {e.Channel.Name} | {e.Channel.Description}", LogType.Network);
        }

        /// <summary>
        ///     Called when we have joined a new chat channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnJoinedChatChannel(object sender, JoinedChatChannelEventArgs e)
        {
            // Find an existing channel with the same name
            var channel = AvailableChatChannels.Find(x => x.Name == e.Channel);

            // If the channel doesn't actually exist in our available ones, that must mean the server is placing
            // us in one that we don't know about, so we'll have to create a new chat channel object.
            if (channel == null)
            {
                var newChannel = new ChatChannel
                {
                    Name = e.Channel,
                    Description = "No Description",
                    AllowedUserGroups = UserGroups.Normal
                };

                JoinedChatChannels.Add(newChannel);
                Logger.LogImportant($"Joined ChatChannel: {e.Channel} which was previously unknown", LogType.Network);

                Dialog.ChatChannelList.InitializeChannel(newChannel);
                return;
            }

            JoinedChatChannels.Add(channel);
            Dialog.ChatChannelList.InitializeChannel(channel);
            Logger.LogImportant($"Joined chat channel: {e.Channel}", LogType.Network);
        }

        /// <summary>
        ///     Called when the server reports that the client has left a chat channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnLeftChatChannel(object sender, LeftChatChannelEventArgs e)
        {
            var channel = JoinedChatChannels.Find(x => x.Name == e.ChannelName);

            // If the channel no longer exists in the joined chat channels, then don't do anything with it.
            if (channel == null)
                return;

            // The previous selected button.
            var oldSelected = Dialog.ChatChannelList.SelectedButton;

            // The channel exists, so we have to kick them out in the overlay.
            var channelButton = Dialog.ChatChannelList.Buttons.Find(x => x.Channel == channel);

            // Make sure the channel is selected temporarily.
            if (channelButton != oldSelected)
                channelButton.SelectChatChannel();

            // Leave the channel
            Dialog.CurrentTopic.CloseActiveChatChannel();

            // Select the old channel that was originally there.
            if (channelButton != oldSelected)
                oldSelected.SelectChatChannel();
        }

        /// <summary>
        ///     Called whenever we receive new chat messages.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            // Don't handle messages from non-online users. We should never get this since the packets are in order.
            if (!OnlineManager.OnlineUsers.ContainsKey(e.Message.SenderId))
                return;

            e.Message.Sender = OnlineManager.OnlineUsers[e.Message.SenderId];

            // Determine if the channel is a private chat or not.
            var isPrivate = !e.Message.Channel.StartsWith("#");

            // If the channel is private, then we want to use the sender's username,
            // otherwise we should use the channel's name.
            var channel = isPrivate ? JoinedChatChannels.Find(x => x.Name == e.Message.Sender.Username)
                                        : JoinedChatChannels.Find(x => x.Name == e.Message.Channel);

            // In the event that the chat channel doesn't already exist, we'll want to add a new one in.
            // (This is for private messages)
            if (channel == null)
            {
                channel = new ChatChannel
                {
                    Name = e.Message.Sender.Username,
                    Description = "Private Message"
                };

                // Don't add non-private channels here.
                if (!channel.IsPrivate)
                    return;

                JoinedChatChannels.Add(channel);
                Dialog.ChatChannelList.InitializeChannel(channel, false);

                Logger.LogImportant($"Added ChatChannel: {channel.Name}, as we have received a message and it did not exist", LogType.Network);
            }

            // Add the message to the appropriate channel.
            channel.Messages.Add(e.Message);

            // Add the message to the container.
            Dialog.ChannelMessageContainers[channel].AddMessage(channel, e.Message);

            // Determine if the channel is unread.
            foreach (var channelButton in Dialog.ChatChannelList.Buttons)
            {
                if (channelButton.Channel == channel && Dialog.ActiveChannel != channel)
                    channelButton.IsUnread = true;
            }

            Logger.LogInfo($"Received a chat message: [{e.Message.Time}] {e.Message.Channel} | {e.Message.Sender.Username} " +
                           $"| {e.Message.SenderId} | {e.Message.Message}", LogType.Network);
        }

        /// <summary>
        ///     Called when the user fails to join a chat channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnFailedToJoinChatChannel(object sender, FailedToJoinChatChannelEventArgs e)
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            NotificationManager.Show(NotificationLevel.Error, $"Failed to join channel: {e.Channel}");
        }

        /// <summary>
        ///     Handles all client sided commands (Messages that start with "/")
        /// </summary>
        /// <param name="message"></param>
        private static void HandleClientSideCommands(ChatMessage message)
        {
            // The args for the message.
            var args = message.Message.Split(' ');

            var command = "";

            var commandSplit = args[0].Split('/');

            if (commandSplit.Length > 1)
                command = commandSplit[1];

            if (string.IsNullOrEmpty(command))
                return;

            switch (command)
            {
                // Send help commands.
                case "help":
                    QuaverBot.ExecuteHelpCommand();
                    break;
                // Get online users.
                case "online":
                    QuaverBot.ExecuteOnlineCommand();
                    break;
                // open up a private chat for a user
                case "chat":
                    QuaverBot.ExecuteChatCommand(args);
                    break;
                // User wants to join a chat cnalle.
                case "join":
                    QuaverBot.ExecuteJoinCommand(args.ToList());
                    break;
            }
        }
    }
}