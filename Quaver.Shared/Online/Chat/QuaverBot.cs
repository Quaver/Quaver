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
using Quaver.Shared.Graphics.Overlays.Chatting;

namespace Quaver.Shared.Online.Chat
{
    public static class QuaverBot
    {
        /// <summary>
        ///     Handles all client sided commands (Messages that start with "/")
        /// </summary>
        /// <param name="message"></param>
        public static void HandleClientSideCommands(ChatMessage message)
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
                    ExecuteHelpCommand();
                    break;
            }
        }

        /// <summary>
        ///     Executes the `/help` command.
        ///
        ///     Shows all of the available client-side commands to the user.
        /// </summary>
        public static void ExecuteHelpCommand()
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            SendMessage(OnlineChat.Instance.ActiveChannel.Value,
                "Hey there, I'm Quaver - a bot that's here to help!\n" +

                "Here are some client-side commands you can use:\n" +
                "/help - Display this message\n" +
                "For server-side commands, you type !help.");
        }


        /// <summary>
        ///     Sends a client-sided QuaverBot message to a specified channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static void SendMessage(ChatChannel channel, string message)
        {
            if (channel == null)
                return;

            var chatMessage = new ChatMessage(channel.Name, message)
            {
                // QuaverBot is id #2
                SenderId = 2,
                Sender = OnlineManager.OnlineUsers[2],
                SenderName = "QuaverBot",
                Time = TimeHelper.GetUnixTimestampMilliseconds()
            };

            channel.QueueMessage(chatMessage);
        }

        /// <summary>
        ///     Sends a message to the user in chat letting them know they're muted.
        /// </summary>
        public static void SendMutedMessage()
        {
            SendMessage(OnlineChat.Instance.ActiveChannel.Value, "Whoa there! Unfortunately you're muted for another: " +
                                             $"{OnlineManager.Self.GetMuteTimeLeftString()}.\n" +
                                            "You won't be able to speak 'till then. Check your profile for more details.");
        }
    }
}
