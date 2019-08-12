/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Form;
using Wobble.Input;

namespace Quaver.Shared.Graphics.Overlays.Chat.Components
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
        public BorderedTextButton SendButton { get; private set; }

        /// <summary>
        ///     Determines if the mute has been initated in the textbox.
        /// </summary>
        private bool MuteInitiatedInTextbox { get; set; }

        /// <summary>
        ///     The list of chat channels the user is currently in.
        /// </summary>
        public List<string> PreviousMessages { get; set; } = new List<string>();

        /// <summary>
        ///     Current showing message
        /// </summary>
        private int CurrentMessage { get; set; } = 0;

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
            Alpha = 0.85f;

            CreateTextbox();
            CreateSendButton();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            Textbox.AlwaysFocused = !Overlay.OnlineUserFilters.SearchTextbox.Focused && ChatManager.MuteTimeLeft <= 0 && Overlay.IsOnTop;
            Textbox.Focused = !Overlay.OnlineUserFilters.SearchTextbox.Focused && ChatManager.MuteTimeLeft <= 0 && Overlay.IsOnTop;

            float targetSendButtonAlpha;

            if (Overlay.ActiveChannel == null)
                targetSendButtonAlpha = 0.85f;
            else if (string.IsNullOrEmpty(Textbox.RawText))
                targetSendButtonAlpha = 0.45f;
            else if (SendButton.IsHovered)
                targetSendButtonAlpha = 1f;
            else
                targetSendButtonAlpha = 0.85f;

            // Reset if empty
            if (Textbox.RawText == "")
                CurrentMessage = 0;

            // Change the textbox's text based on
            if (!Textbox.Focused && ChatManager.MuteTimeLeft > 0)
            {
                // var t = TimeSpan.FromMilliseconds(ChatManager.MuteTimeLeft);
                // Textbox.InputText.Text = $"You are currently muted for another {t.Days} days {t.Hours} hours {t.Minutes} minutes and {t.Seconds} seconds.";
                // Textbox.InputText.Tint = Color.OrangeRed;
                MuteInitiatedInTextbox = true;
            }
            else if (MuteInitiatedInTextbox)
            {
                Textbox.InputText.Alpha = 0.5f;
                Textbox.InputText.Text = "Type to send a message";
                MuteInitiatedInTextbox = false;
            }
            else if (KeyboardManager.IsUniqueKeyPress(Keys.Up) && PreviousMessages.Count > 0 && CurrentMessage < PreviousMessages.Count)
            {
                CurrentMessage++;
                Textbox.RawText = PreviousMessages[PreviousMessages.Count - CurrentMessage];
            }
            else if (KeyboardManager.IsUniqueKeyPress(Keys.Down) && PreviousMessages.Count > 0 && CurrentMessage > 0)
            {
                CurrentMessage--;

                // Reset if no other saved messages
                if (CurrentMessage == 0)
                    Textbox.RawText = "";
                else
                    Textbox.RawText = PreviousMessages[PreviousMessages.Count - CurrentMessage];
            }

            SendButton.Alpha = MathHelper.Lerp(SendButton.Alpha, targetSendButtonAlpha, (float) Math.Min(dt / 60f, 1));

            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the Textbox sprite.
        /// </summary>
        private void CreateTextbox()
        {
            Textbox = new Textbox(new ScalableVector2(1100, Height * 0.60f), Fonts.Exo2Regular,
                13, "", "Type to send a message", OnTextboxSubmit)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 10,
                Image = UserInterface.BlankBox,
                Tint = Color.Black,
                Alpha = 0.35f,
                AlwaysFocused = true,
                MaxCharacters = 2000
            };

            Textbox.InputText.Y = 8;
            Textbox.Cursor.Y += Textbox.InputText.Y;
            Textbox.AddBorder(Color.White, 2);
        }

        /// <summary>
        ///     Called when the textbox is submitted.
        /// </summary>
        /// <param name="text"></param>
        private void OnTextboxSubmit(string text)
        {
            if (Overlay.ActiveChannel == null)
                return;

            var chatMessage = new ChatMessage(Overlay.ActiveChannel.Name, text);
            ChatManager.SendMessage(Overlay.ActiveChannel, chatMessage);

            // Delete element if message is found
            if (PreviousMessages.Contains(text))
                PreviousMessages.Remove(text);

            // Save messages
            PreviousMessages.Add(text);

            // Scroll to the bottom when sending chat messages
            var messageContainer = Overlay.ChannelMessageContainers[Overlay.ActiveChannel];
            messageContainer.ScrollTo(-messageContainer.ContentContainer.Height, 800);
        }

        /// <summary>
        ///     Creates the button to send chage messages.
        /// </summary>
        private void CreateSendButton() => SendButton = new BorderedTextButton("Send Message", Colors.MainAccent, (sender, args) =>
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
            Text =
                {
                    FontSize = 13,
                    UsePreviousSpriteBatchOptions = true,
                    Font = Fonts.SourceSansProSemiBold
                }
        };
    }
}
