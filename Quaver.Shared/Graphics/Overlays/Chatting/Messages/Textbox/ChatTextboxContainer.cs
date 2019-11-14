using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Messages.Textbox
{
    public class ChatTextboxContainer : Sprite
    {
        /// <summary>
        /// </summary>
        private Bindable<ChatChannel> ActiveChannel { get; }

        /// <summary>
        /// </summary>
        private OnlineChatTextbox ChatTextbox { get; set; }

        /// <summary>
        /// </summary>
        private IconButton EmojiButton { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="activeChannel"></param>
        /// <param name="size"></param>
        public ChatTextboxContainer(Bindable<ChatChannel> activeChannel, ScalableVector2 size)
        {
            ActiveChannel = activeChannel;

            Size = size;
            Alpha = 1;
            Tint = ColorHelper.HexToColor("#292929");

            CreateTextbox();
            CreateEmojiButton();
        }

        /// <summary>
        /// </summary>
        private void CreateTextbox()
        {
            ChatTextbox = new OnlineChatTextbox(ActiveChannel, new ScalableVector2(Width * 0.93f, Height * 0.62f))
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 16
            };
        }

        /// <summary>
        /// </summary>
        private void CreateEmojiButton()
        {
            EmojiButton = new IconButton(UserInterface.Emoji)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = ChatTextbox.X + ChatTextbox.Width + 14,
                Y = 3,
                Size = new ScalableVector2(38, 38)
            };
        }
    }
}