using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Overlays.Chatting.Channels.Scrolling;
using Quaver.Shared.Helpers;
using TagLib.Id3v2;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Channels
{
    public class ChatChannelList : Sprite, IResizable
    {
        /// <summary>
        /// </summary>
        private Bindable<ChatChannel> ActiveChannel { get; }

        /// <summary>
        /// </summary>
        public Sprite HeaderBackground { get; private set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus HeaderText { get; set; }

        /// <summary>
        /// </summary>
        private JoinChatButton JoinChatButton { get; set; }

        /// <summary>
        /// </summary>
        public ChatChannelScrollContainer ChannelContainer { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="activeChannel"></param>
        /// <param name="size"></param>
        public ChatChannelList(Bindable<ChatChannel> activeChannel, ScalableVector2 size)
        {
            ActiveChannel = activeChannel;

            Tint = ColorHelper.HexToColor("#242424");
            Size = size;

            CreateHeaderBackground();
            CreateHeaderText();
            CreateJoinChatButton();
            CreateChannelContainer();
        }

        /// <summary>
        /// </summary>
        private void CreateHeaderBackground()
        {
            HeaderBackground = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(Width, 56),
                Image = UserInterface.HubHeaderBackground
            };
        }

        /// <summary>
        /// </summary>
        private void CreateHeaderText()
        {
            HeaderText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Chat Channels".ToUpper(), 21)
            {
                Parent = HeaderBackground,
                Alignment = Alignment.MidLeft,
                X = 16
            };
        }

        /// <summary>
        /// </summary>
        private void CreateJoinChatButton()
        {
            JoinChatButton = new JoinChatButton()
            {
                Parent = HeaderBackground,
                Alignment = Alignment.MidRight,
                X = -HeaderText.X,
                Size = new ScalableVector2(20, 20)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateChannelContainer()
        {
            ChannelContainer = new ChatChannelScrollContainer(ActiveChannel, HeaderBackground.Height,
                new ScalableVector2(Width, Height - HeaderBackground.Height))
            {
                Parent = this,
                Y = HeaderBackground.Height
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public void ChangeSize(ScalableVector2 size)
        {
            Height = size.X.Value;

            foreach (var child in Children)
            {
                if (child is IResizable c)
                    c.ChangeSize(size);
            }
        }
    }
}