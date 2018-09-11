using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using osu_database_reader;
using Quaver.Graphics.Overlays.Chat;
using Quaver.Graphics.Overlays.Chat.Components.Messages.Drawable;
using Quaver.Logging;
using Quaver.Scheduling;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Online
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


            if (KeyboardManager.IsUniqueKeyPress(Keys.F8) && TimeSinceLastActivated >= 450)
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

            // Add the message to the appropriate channel.
            channel.Messages.Add(message);

            // Add the message to the container.
            Dialog.ChannelMessageContainers[channel].AddMessage(channel, message);

            // Send the message to the server.
            OnlineManager.Client.SendMessage(chan.Name, message.Message);
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

            // Find the channel the message is for.
            var channel = JoinedChatChannels.Find(x => x.Name == e.Message.Channel);

            // In the event that the chat channel doesn't already exist, we'll want to add a new one in.
            if (channel == null)
            {
                channel = new ChatChannel
                {
                    Name = e.Message.Channel
                };

                JoinedChatChannels.Add(channel);
                Dialog.ChatChannelList.InitializeChannel(channel);
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

            Logger.LogInfo($"Received a chat message: [{e.Message.Time}] {e.Message.Channel} | {e.Message.Sender.Username} | {e.Message.SenderId} | {e.Message.Message}", LogType.Network);
        }
    }
}