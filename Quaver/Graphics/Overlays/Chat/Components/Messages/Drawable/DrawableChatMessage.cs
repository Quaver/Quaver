using Quaver.Assets;
using Quaver.Online;
using Quaver.Server.Client.Structures;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Graphics.Overlays.Chat.Components.Messages.Drawable
{
    public class DrawableChatMessage : Sprite
    {
        /// <summary>
        ///     The user's avatar.
        /// </summary>
        public CircleAvatar Avatar { get; }

        /// <summary>
        ///     The actual chat message.
        /// </summary>
        public ChatMessage Message { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        public DrawableChatMessage(ChatMessage message)
        {
            Avatar = new CircleAvatar(new ScalableVector2(45, 45), UserInterface.YouAvatar)
            {
                Parent = this,
                X = 15
            };

            Message = message;
        }
    }
}