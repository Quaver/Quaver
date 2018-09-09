using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Graphics.Overlays.Chat.Components.Messages
{
    public class ChatMessageContainer : ScrollContainer
    {
        /// <summary>
        ///     Reference to the parent overlay.
        /// </summary>
        private ChatOverlay Overlay { get; }

        /// <summary>
        ///     The channel this container is for.
        /// </summary>
        private ChatChannel Channel { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ChatMessageContainer(ChatOverlay overlay, ChatChannel channel)
            : base(new ScalableVector2(overlay.MessageContainer.Width, overlay.MessageContainer.Height - overlay.CurrentTopicContainer.Height),
                new ScalableVector2(overlay.MessageContainer.Width, overlay.MessageContainer.Height - overlay.CurrentTopicContainer.Height + 1))
        {
            Overlay = overlay;
            Channel = channel;

            Parent = overlay.MessageContainer;
            SetChildrenVisibility = true;
            Y = Overlay.CurrentTopicContainer.Height;
            Tint = Color.Black;


            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 8;
            Scrollbar.X -= 1;
        }
    }
}