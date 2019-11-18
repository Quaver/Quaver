using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Form.Checkboxes;
using Quaver.Shared.Online;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Channels.Join
{
    public class JoinChatChannelCheckboxItem : ICheckboxContainerItem
    {
        /// <summary>
        /// </summary>
        private ChatChannel Channel { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="chan"></param>
        public JoinChatChannelCheckboxItem(ChatChannel chan) => Channel = chan;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public string GetName() => Channel.Name;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public bool GetSelectedState() => OnlineChat.JoinedChatChannels.Contains(Channel);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void OnToggle()
        {
            if (GetSelectedState())
                OnlineManager.Client?.LeaveChatChannel(Channel);
            else
                OnlineManager.Client?.JoinChatChannel(Channel.Name);
        }
    }
}