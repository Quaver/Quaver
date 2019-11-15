using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Channels.Scrolling
{
    public class ChatChannelRightClickOptions : RightClickOptions
    {
        /// <summary>
        /// </summary>
        private Bindable<ChatChannel> ActiveChannel { get; }

        /// <summary>
        /// </summary>
        private ChatChannelScrollContainer Container { get; }

        /// <summary>
        /// </summary>
        private ChatChannel Channel { get; }

        private const string OpenText = "Open";

        private const string SaveChatLog = "Save Chat Log";

        private const string CloseText = "Close";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="activeChannel"></param>
        /// <param name="container"></param>
        public ChatChannelRightClickOptions(ChatChannel channel, Bindable<ChatChannel> activeChannel, ChatChannelScrollContainer container)
            : base(new Dictionary<string, Color>()
        {
            {OpenText, Color.White},
            {SaveChatLog, ColorHelper.HexToColor("#0787E3")},
            {CloseText, ColorHelper.HexToColor($"#FF6868")}
        }, new ScalableVector2(200, 40), 22)
        {
            ActiveChannel = activeChannel;
            Container = container;
            Channel = channel;

            ItemSelected += (sender, args) =>
            {
                switch (args.Text)
                {
                    case OpenText:
                        ActiveChannel.Value = Channel;
                        break;
                    case SaveChatLog:
                        ThreadScheduler.Run(() => OnlineChat.SaveChatLog(Channel));
                        break;
                    case CloseText:
                        Channel.Close();

                        if (!Channel.IsPrivate)
                            OnlineManager.Client?.LeaveChatChannel(Channel);
                        break;
                }
            };
        }
    }
}