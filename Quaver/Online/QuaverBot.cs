using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using Quaver.Config;
using Quaver.Graphics.Notifications;
using Quaver.Server.Client.Structures;

namespace Quaver.Online
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
                "/chat <username> - Open a private chat with a user");
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
                userStr += user.Value.Username + ", ";

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

            // Check to see if that player is actually online.
            var foundUser = OnlineManager.OnlineUsers.Values.ToList().Find(x => string.Equals(x.Username,
                                    username, StringComparison.CurrentCultureIgnoreCase));

            if (foundUser != null)
            {
                Console.WriteLine($"User: {foundUser.Username} is online!");

                // Check to see if the chat already is open, in that case just open it.
                var joinedChannel = ChatManager.JoinedChatChannels.Find(x => string.Equals(x.Name, foundUser.Username,
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
                        Name = foundUser.Username,
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
        ///     Sends a client-sided QuaverBot message to a specified channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static void SendMessage(ChatChannel channel, string message)
        {
            var chatMessage = new ChatMessage(channel.Name, message)
            {
                // QuaverBot is ID = 0;
                SenderId = 0,
                Sender = OnlineManager.OnlineUsers[0]
            };

            // Add the message to the appropriate channel.
            channel.Messages.Add(chatMessage);

            ChatManager.Dialog.ChannelMessageContainers[channel].AddMessage(channel, chatMessage);
        }
    }
}