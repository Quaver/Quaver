using System;
using System.Drawing;
using Quaver.Assets;
using Quaver.Graphics.Notifications;
using Quaver.Online;
using Quaver.Server.Client.Structures;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Graphics.Overlays.Chat.Components.Messages.Drawable
{
    public class DrawableChatMessage : Sprite
    {
        /// <summary>
        ///     The parent chat message container.
        /// </summary>
        public ChatMessageContainer Container { get; }

        /// <summary>
        ///     The user's avatar.
        /// </summary>
        public Sprite Avatar { get; }

        /// <summary>
        ///     The actual chat message.
        /// </summary>
        public ChatMessage Message { get; }

        /// <summary>
        ///     The username of the person that wrote the message.
        /// </summary>
        public SpriteText TextUsername { get; private set; }

        /// <summary>
        ///     The actual content of the message.
        /// </summary>
        public SpriteText TextMessageContent { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="message"></param>
        public DrawableChatMessage(ChatMessageContainer container, ChatMessage message)
        {
            Container = container;
            Message = message;

            Avatar = new Sprite
            {
                Parent = this,
                X = 15,
                Size = new ScalableVector2(40, 40),
                Image = UserInterface.YouAvatar
            };

            Y = 100;

            CreateUsernameText();
            CreateMessageContentText();
        }

        /// <summary>
        ///    Creates the text of the user who wrote the message.
        /// </summary>
        private void CreateUsernameText()
        {
            TextUsername = new SpriteText(Fonts.Exo2BoldItalic24, Message.Sender.Username, 0.55f)
            {
                Parent = this,
                TextColor = Colors.GetUserChatColor(Message.Sender)
            };

            TextUsername.X = TextUsername.MeasureString().X / 2f + Avatar.Width + Avatar.X + 12;
            TextUsername.Y = TextUsername.MeasureString().Y / 2f - 3;
        }

        /// <summary>
        ///
        /// </summary>
        private void CreateMessageContentText()
        {
            TextMessageContent = new SpriteText(Fonts.Exo2Regular24, Message.Message,
                new ScalableVector2(Container.Width - Avatar.Width - 40, 1), 0.55f)
            {
                Parent = this,
                Style = TextStyle.WordwrapMultiLine,
                X = Avatar.X + Avatar.Width + 12,
                TextAlignment = Alignment.TopLeft
            };

            TextMessageContent.Y = TextMessageContent.MeasureString().Y / 2f + 8;
            Console.WriteLine(TextMessageContent.Alignment + " " + TextMessageContent.TextAlignment + " " + TextMessageContent.Y);
            Console.WriteLine(TextMessageContent.Text);
        }
    }
}