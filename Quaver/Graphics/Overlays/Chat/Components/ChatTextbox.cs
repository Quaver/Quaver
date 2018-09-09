using System;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Online;
using Quaver.Server.Client.Structures;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Form;

namespace Quaver.Graphics.Overlays.Chat.Components
{
    public class ChatTextbox : Sprite
    {
        /// <summary>
        ///     Reference ot the parent chat overlay.
        /// </summary>
        private ChatOverlay Overlay { get; }

        /// <summary>
        ///     The actual textbox used to type messages into.
        /// </summary>
        public Textbox Textbox { get; private set; }

        /// <summary>
        ///     The button used to alternatively send the message.
        /// </summary>
        public TextButton SendButton { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="overlay"></param>
        public ChatTextbox(ChatOverlay overlay)
        {
            Overlay = overlay;

            Parent = Overlay.TextboxContainer;
            Size = Overlay.TextboxContainer.Size;
            Tint = Color.Black;

            CreateTextbox();
            CreateSendButton();
        }

        /// <summary>
        ///     Creates the Textbox sprite.
        /// </summary>
        private void CreateTextbox()
        {
            Textbox = new Textbox(TextboxStyle.SingleLine, new ScalableVector2(600, Height * 0.60f),
                Fonts.Exo2Regular24,
                "", "Type to send a message", 0.60f, OnTextboxSubmit)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 10,
                Image = UserInterface.UsernameSelectionTextbox,
                AlwaysFocused = true
            };

            Textbox.InputText.Y = 8;
            Textbox.Cursor.Y += Textbox.InputText.Y;
        }

        /// <summary>
        ///     Called when the textbox is submitted.
        /// </summary>
        /// <param name="text"></param>
        private void OnTextboxSubmit(string text)
        {
            var chatMessage = new ChatMessage(Overlay.ActiveChannel.Name, text);
            ChatManager.SendMessage(Overlay.ActiveChannel, chatMessage);
        }

        /// <summary>
        ///     Creates the button to send messages.
        /// </summary>
        private void CreateSendButton() => SendButton = new TextButton(UserInterface.BlankBox, Fonts.Exo2Regular24, "Send", 0.65f,
            (o, e) =>
            {
                if (string.IsNullOrEmpty(Textbox.RawText))
                    return;

                Textbox.OnSubmit?.Invoke(Textbox.RawText);
                Textbox.RawText = string.Empty;
                Textbox.ReadjustTextbox();
            })
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            Size = new ScalableVector2(Width - Textbox.Width - 30, Textbox.Height),
            X = Textbox.Width + 20,
            Tint = Color.Red
        };
    }
}