/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Helpers;
using Quaver.Shared.Graphics.Notifications;

namespace Quaver.Shared.Online.Chat
{
    public static class QuaverBot
    {
        /// <summary>
        ///     Executes the `/help` command.
        ///
        ///     Shows all of the available client-side commands to the user.
        /// </summary>
        public static void ExecuteHelpCommand()
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            SendMessage(ChatManager.Dialog.ActiveChannel,
                "Hey there, I'm Quaver - a bot that's here to help!\n\n" +

                "Here are some client-side commands you can use:\n" +
                "/help - Display this message\n" +
                "/online - Display all online users\n" +
                "/chat <username> - Open a private chat with a user\n" +
                "/join <channel name> - Request to join a chat channel.\n");
            // "/spectate <username> - Start spectating a player");
        }

        /// <summary>
        ///     Executes the `/online` command.
        ///
        ///     Displays all of the currently online users.
        /// </summary>
        public static void ExecuteOnlineCommand()
        {
            var userStr = "";

            foreach (var user in OnlineManager.OnlineUsers)
                userStr += user.Value.OnlineUser.Username + ", ";

            SendMessage(ChatManager.Dialog.ActiveChannel,
                $"There are {OnlineManager.OnlineUsers.Count} users online.\n\n" +
                $"{userStr}");
        }

        /// <summary>
        ///    Executes the `/chat` command.
        ///
        ///    Opens up a chat with the specified user.
        /// </summary>
        public static void ExecuteChatCommand(IEnumerable<string> args)
        {
            var argsList = new List<string>(args);
            argsList.RemoveAt(0);

            if (argsList.Count == 0)
                return;

            // Get the username of the user.
            var username = string.Join(" ", argsList);

            // Don't allow non-usernames to have this functionality.
            if (username.StartsWith("#"))
                return;

            // Check to see if that player is actually online
            // var foundUser = new User(OnlineManager.OnlineUsers.Count + 1, -1, username, UserGroups.Normal);
            var foundUser = OnlineManager.OnlineUsers.Values.ToList().Find(x => string.Equals(x.OnlineUser.Username,
                                    username, StringComparison.CurrentCultureIgnoreCase));

            // User is online, so we'll need to join/switch to that chat channel.
            if (foundUser != null)
            {
                // Check to see if the chat already is open, in that case just open it.
                var joinedChannel = ChatManager.JoinedChatChannels.Find(x => string.Equals(x.Name, foundUser.OnlineUser.Username,
                                        StringComparison.CurrentCultureIgnoreCase));

                // In the event that the channel is already open, we want to just switch to it.
                if (joinedChannel != null)
                {
                    var openedChannel = ChatManager.Dialog.ChatChannelList.Buttons.Find(x => x.Channel == joinedChannel);
                    openedChannel.SelectChatChannel();
                }
                // Chat is clear to open.
                else
                {
                    var newChannel = new ChatChannel
                    {
                        Name = foundUser.OnlineUser.Username,
                        Description = "Private Message"
                    };

                    ChatManager.JoinedChatChannels.Add(newChannel);
                    ChatManager.Dialog.ChatChannelList.InitializeChannel(newChannel);
                }
            }
            // The user is offline, so just send a message from QuaverBot in chat.
            else
            {
                SendMessage(ChatManager.Dialog.ActiveChannel, $"Cannot open a chat with \"{username}\" because they are offline.");
            }
        }

        /// <summary>
        ///     Executes the '/join' command
        /// </summary>
        public static void ExecuteJoinCommand(IEnumerable<string> args)
        {
            var argsList = new List<string>(args);
            argsList.RemoveAt(0);

            if (argsList.Count == 0)
                return;

            // Get the name of the channel the user wishes to join
            var channel = argsList[0];

            // Only allow valid channel names to be joinable.
            if (!channel.StartsWith("#"))
            {
                NotificationManager.Show(NotificationLevel.Error, "Chat channel names must start with a #");
                return;
            }

            // Find if we're already in this given channel.
            var alreadyJoinedChannel = ChatManager.JoinedChatChannels.Find(x => x.Name == channel);

            // If we are in the channel, then simply change to it.
            if (alreadyJoinedChannel != null)
            {
                ChatManager.Dialog.ChatChannelList.Buttons.Find(x => x.Channel == alreadyJoinedChannel)?.SelectChatChannel();
                return;
            }

            // The channel has to be available in order to be able join join it.
            if (ChatManager.AvailableChatChannels.All(x => x.Name != channel))
            {
                NotificationManager.Show(NotificationLevel.Error, "That channel is unavailable to join.");
                return;
            }

            OnlineManager.Client.JoinChatChannel(channel);
        }

        /// <summary>
        ///     Sends a client-sided QuaverBot message to a specified channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static void SendMessage(ChatChannel channel, string message)
        {
            var chatMessage = new ChatMessage(channel.Name, message)
            {
                // QuaverBot is id #2
                SenderId = 2,
                Sender = OnlineManager.OnlineUsers[2],
                SenderName = "QuaverBot",
                Time = TimeHelper.GetUnixTimestampMilliseconds()
            };

            // Add the message to the appropriate channel.
            channel.Messages.Add(chatMessage);

            ChatManager.Dialog.ChannelMessageContainers[channel].AddMessage(channel, chatMessage);
        }

        /// <summary>
        ///     Sends a message to the user in chat letting them know they're muted.
        /// </summary>
        public static void SendMutedMessage()
        {
            if (ChatManager.Dialog.ActiveChannel == null)
                return;

            SendMessage(ChatManager.Dialog.ActiveChannel, $"Whoa there! Unfortunately you're muted for another: " +
                                             $"{OnlineManager.Self.GetMuteTimeLeftString()}.\n" +
                                            $"You won't be able to speak 'till then. Check your profile for more details.");
        }

                /// <summary>
        ///    Executes the `/spectate`
        ///
        ///    Opens up a chat with the specified user.
        /// </summary>
        public static void ExecuteSpectateCommand(IEnumerable<string> args)
        {
            var argsList = new List<string>(args);
            argsList.RemoveAt(0);

            if (argsList.Count == 0)
                return;

            // Get the username of the user.
            var username = string.Join(" ", argsList);

            // Don't allow non-usernames to have this functionality.
            if (username.StartsWith("#"))
                return;

            // Check to see if that player is actually online
            // var foundUser = new User(OnlineManager.OnlineUsers.Count + 1, -1, username, UserGroups.Normal);
            var foundUser = OnlineManager.OnlineUsers.Values.ToList().Find(x => string.Equals(x.OnlineUser.Username,
                                    username, StringComparison.CurrentCultureIgnoreCase));

            // User is online, so we'll need to join/switch to that chat channel.
            if (foundUser != null)
                OnlineManager.Client?.SpectatePlayer(foundUser.OnlineUser.Id);
            else
                SendMessage(ChatManager.Dialog.ActiveChannel, "That user is not online!");
        }

        /// <summary>
        ///     Executes the `/stopspectating` command.
        /// </summary>
        public static void ExecuteStopSpectatingCommand() => OnlineManager.Client?.StopSpectating();
    }
}
