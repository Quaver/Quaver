using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form.Checkboxes;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Chatting.Channels;
using Quaver.Shared.Graphics.Overlays.Chatting.Channels.Join;
using Quaver.Shared.Graphics.Overlays.Chatting.Messages;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Platform;
using Wobble.Window;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Graphics.Overlays.Chatting
{
    public class OnlineChat : Sprite, IResizable
    {
        /// <summary>
        /// </summary>
        public Bindable<ChatChannel> ActiveChannel { get; } = new Bindable<ChatChannel>(null);

        /// <summary>
        ///     List of chat channels that are available to join
        /// </summary>
        public static List<ChatChannel> AvailableChatChannels { get; } = new List<ChatChannel>();

        /// <summary>
        ///     The list of chat channels that the user has joined
        /// </summary>
        public static List<ChatChannel> JoinedChatChannels { get; } = new List<ChatChannel>();

        /// <summary>
        /// </summary>
        public ChatChannelList ChannelList { get; private set; }

        /// <summary>
        /// </summary>
        public ChatMessageContainer MessageContainer { get; private set; }

        /// <summary>
        /// </summary>
        public CheckboxContainer ActiveJoinChatChannelContainer { get; private set; }

        /// <summary>
        ///     If the chat overlay is opened
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// </summary>
        public static OnlineChat Instance
        {
            get
            {
                var game = (QuaverGame) GameBase.Game;
                return game.OnlineChat;
            }
        }

        /// <summary>
        ///     If the user is currently resizing the chat
        /// </summary>
        public bool IsResizing { get; private set; }

        /// <summary>
        /// </summary>
        public OnlineChat()
        {
            Size = new ScalableVector2(WindowManager.Width - OnlineHub.WIDTH, 450);
            Tint = ColorHelper.HexToColor("#2F2F2F");

            CreateChatChannelList();
            CreateChatMessageContainer();

            ChannelList.Parent = this;
            DestroyIfParentIsNull = false;

            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleResizing();
            HandleActiveJoinChatChannelDialogClosing();

            base.Update(gameTime);
        }

        /// <summary>
        ///     Performs an animation to open the chat
        /// </summary>
        public void Open()
        {
            ClearAnimations();
            MoveToY(0, Easing.OutQuint, 500);
            IsOpen = true;
        }

        /// <summary>
        ///     Performs an animation to close the clear
        /// </summary>
        public void Close()
        {
            ClearAnimations();
            MoveToY((int) Height + 10, Easing.OutQuint, 500);
            IsOpen = false;
        }

        /// <summary>
        ///     Returns if the dialog to join chat channels is open
        /// </summary>
        /// <returns></returns>
        public bool IsJoinChannelDialogOpen() => ActiveJoinChatChannelContainer != null && ActiveJoinChatChannelContainer.IsOpen;

        /// <summary>
        /// </summary>
        private void HandleResizing()
        {
            if (!ChannelList.HeaderBackground.IsHeld && !MessageContainer.TopicHeader.IsHeld)
            {
                IsResizing = false;
                return;
            }

            IsResizing = true;

            var min = MessageContainer.TextboxContainer.Height + 200;
            var max = WindowManager.Height - MenuBorder.HEIGHT;
            var height = WindowManager.Height - MouseManager.CurrentState.Y + ChannelList.HeaderBackground.Height / 2f;

            ChangeSize(new ScalableVector2(Width, MathHelper.Clamp(height, min, max)));
        }

        /// <summary>
        ///  Handles when the user clicks outside of the join chat dialog
        /// </summary>
        private void HandleActiveJoinChatChannelDialogClosing()
        {
            if (MouseManager.IsUniqueClick(MouseButton.Left) && ActiveJoinChatChannelContainer != null
                                                             && !ActiveJoinChatChannelContainer.IsHovered())
            {
                ActiveJoinChatChannelContainer.Close();
            }
        }

        /// <summary>
        /// </summary>
        private void CreateChatChannelList()
            => ChannelList = new ChatChannelList(ActiveChannel, new ScalableVector2(250, Height)) {Parent = this};

        /// <summary>
        /// </summary>
        private void CreateChatMessageContainer()
        {
            MessageContainer = new ChatMessageContainer(ActiveChannel, new ScalableVector2(Width - ChannelList.Width, Height))
            {
                Parent = this,
                X = ChannelList.Width
            };
        }

        /// <summary>
        ///     Handles changing the size of the chat
        /// </summary>
        /// <param name="size"></param>
        public void ChangeSize(ScalableVector2 size)
        {
            Size = size;

            foreach (var child in Children)
            {
                if (child is IResizable c)
                    c.ChangeSize(size);
            }
        }

        /// <summary>
        ///    Activates the container to join chat channels
        /// </summary>
        public void ActivateJoinChatChannelList()
        {
            ActiveJoinChatChannelContainer?.Destroy();

            var channels = new List<ICheckboxContainerItem>();
            AvailableChatChannels.ForEach(x => channels.Add(new JoinChatChannelCheckboxItem(x)));

            ActiveJoinChatChannelContainer = new CheckboxContainer(channels, new ScalableVector2(225, 400), 200)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Y = ChannelList.HeaderBackground.Height / 2f
            };

            ActiveJoinChatChannelContainer.X = ActiveJoinChatChannelContainer.Width + 24;
        }

        /// <summary>
        ///     Subscribes to online events when logging online
        /// </summary>
        private void SubscribeToOnlineEvents()
        {
            OnlineManager.Client.OnAvailableChatChannel += OnAvailableChatchannel;
            OnlineManager.Client.OnFailedToJoinChatChannel += OnFailedToJoinChatChannel;
        }

        /// <summary>
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (OnlineManager.Client == null)
                return;

            OnlineManager.Client.OnAvailableChatChannel -= OnAvailableChatchannel;
            OnlineManager.Client.OnFailedToJoinChatChannel -= OnFailedToJoinChatChannel;
        }

        /// <summary>
        ///     When successfully connecting, subscribe to online chat related events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            if (e.Value != ConnectionStatus.Connected)
            {
                UnsubscribeFromEvents();
                return;
            }

            SubscribeToOnlineEvents();

            AvailableChatChannels.Clear();
            Logger.Important("Cleared previously available chat channels", LogType.Runtime);

            foreach (var chan in JoinedChatChannels)
            {
                if (chan.IsPrivate)
                    continue;

                OnlineManager.Client?.JoinChatChannel(chan.Name);
                Logger.Important($"Requested to rejoin chat channel: {chan.Name}", LogType.Runtime);
            }
        }

        /// <summary>
        ///     Called when receiving a new available chat channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAvailableChatchannel(object sender, AvailableChatChannelEventArgs e)
        {
            if (AvailableChatChannels.Contains(e.Channel))
                return;

            AvailableChatChannels.Add(e.Channel);
            Logger.Important($"Received available chat channel: {e.Channel.Name}", LogType.Runtime);
        }

        /// <summary>
        ///     Called when failing to join a chat channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFailedToJoinChatChannel(object sender, FailedToJoinChatChannelEventArgs e)
        {
            var log = $"Failed to join channel: {e.Channel}";

            NotificationManager.Show(NotificationLevel.Error, log);
            Logger.Important(log, LogType.Runtime);
        }

        /// <summary>
        ///     Saves a chat log to a file and opens it
        /// </summary>
        /// <param name="channel"></param>
        public static void SaveChatLog(ChatChannel channel)
        {
            if (channel.Messages.Count == 0)
            {
                NotificationManager.Show(NotificationLevel.Warning, "This chat channel contains no messages.");
                return;
            }

            var messageStr = new StringBuilder();

            messageStr.AppendLine($"{channel.Name} chat log - {DateTime.Now.ToLongDateString()} @ {DateTime.Now.ToLongTimeString()}");
            messageStr.AppendLine();

            foreach (var message in channel.Messages)
            {
                var dateTime = DateTimeOffset.FromUnixTimeMilliseconds((long) message.Time);
                var time = $"{dateTime.Hour:00}:{dateTime.Minute:00}:{dateTime.Second:00}";

                messageStr.AppendLine($"[{time}] {message.SenderName}: {message.Message}");
            }

            if (ConfigManager.DataDirectory == null)
                return;

            var dir = $"{ConfigManager.DataDirectory}/Chat/";
            Directory.CreateDirectory(dir);

            var name = $"{channel.Name}-{DateTime.Now.ToShortDateString()}-{DateTime.Now.ToShortTimeString()}.txt";
            var path = $"{dir}{StringHelper.FileNameSafeString(name)}";

            using (var sw = new StreamWriter(path))
                sw.Write(messageStr);

            Utils.NativeUtils.HighlightInFileManager(path);
            NotificationManager.Show(NotificationLevel.Success, $"Successfully saved {channel.Name}'s chat log!");
        }

        /// <summary>
        ///     Returns if the channel is considered special
        /// </summary>
        /// <param name="chan"></param>
        /// <returns></returns>
        public static bool IsSpecialChannel(ChatChannel chan)
        {
            if (chan == null)
                return true;

            if (chan.Name.StartsWith("#spectator"))
                return true;
            if (chan.Name.StartsWith("#multi"))
                return true;

            return false;
        }

        /// <summary>
        ///     Example chat channels used for testing
        /// </summary>
        /// <returns></returns>
        private static List<ChatChannel> GetTestChannels()
        {
            var channels = new List<ChatChannel>();

            channels.Add(new ChatChannel()
            {
                Name = "#announcements",
                Description = "No Description"
            });

            channels.Add(new ChatChannel()
            {
                Name = "#quaver",
                Description = "No Description"
            });

            channels.Add(new ChatChannel()
            {
                Name = "#offtopic",
                Description = "No Description"
            });

            /*for (var i = 0; i < 10; i++)
            {
                channels.Add(new ChatChannel()
                {
                    Name = $"#example-{i}",
                    Description = $"Example Channel #{i}",
                    IsUnread = true,
                    IsMentioned = true
                });
            }*/

            return channels;
        }
    }
}