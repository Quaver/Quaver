/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Logging;
using Color = Microsoft.Xna.Framework.Color;
using Logger = Wobble.Logging.Logger;

namespace Quaver.Shared.Graphics.Overlays.Chat.Components.Messages.Drawable
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
        ///     A chat badge (if the user has one)
        /// </summary>
        public ChatBadge ChatBadge { get; private set; }

        /// <summary>
        ///     The actual content of the message.
        /// </summary>
        public SpriteText TextMessageContent { get; private set; }

        /// <summary>
        ///     The amount of y space between the content and time sent.
        /// </summary>
        private int Padding { get; } = 10;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="message"></param>
        public DrawableChatMessage(ChatMessageContainer container, ChatMessage message)
        {
            Container = container;
            Message = message;
            DestroyIfParentIsNull = false;

            Avatar = new Sprite
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                X = 10,
                Size = new ScalableVector2(44, 44),
                Y = Padding,
            };

            SteamManager.SteamUserAvatarLoaded += OnSteamAvatarLoaded;

            // QuaverBot. No need to load.
            if (message.Sender.OnlineUser.SteamId == 0)
                Avatar.Image = UserInterface.UnknownAvatar;
            // We've got the user's avatar, so use it.
            if (SteamManager.UserAvatars.ContainsKey((ulong) message.Sender.OnlineUser.SteamId))
                Avatar.Image = SteamManager.UserAvatars[(ulong) message.Sender.OnlineUser.SteamId];
            // Need to retrieve user's avatar.
            else
            {
                // Go with an unknown avatar for now until it's loaded.
                Avatar.Image = UserInterface.UnknownAvatar;
                SteamManager.SendAvatarRetrievalRequest((ulong) message.Sender.OnlineUser.SteamId);
            }

            var userColor = Colors.GetUserChatColor(Message.Sender.OnlineUser.UserGroups);

            Avatar.AddBorder(new Color(userColor.R  / 2, userColor.G / 2, userColor.B / 2), 2);
            Avatar.Border.Alpha = 0.95f;

            X = -Container.Width;
            Width = Container.Width - 5;
            Alpha = 0;

            CreateUsernameText();
            CreateChatBadge();
            CreateMessageContentText();
            RecalculateHeight();
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            SteamManager.SteamUserAvatarLoaded -= OnSteamAvatarLoaded;

            base.Destroy();
        }

        /// <summary>
        ///    Creates the text of the user who wrote the message.
        /// </summary>
        private void CreateUsernameText()
        {
            var timespan = TimeSpan.FromMilliseconds(Message.Time);
            var date = (new DateTime(1970, 1, 1) + timespan).ToLocalTime();

            var un = Message.Sender.OnlineUser.Username == OnlineManager.Self.OnlineUser.Username
                ? OnlineManager.Self.OnlineUser.Username
                : Message.SenderName;

            TextUsername = new SpriteText(Fonts.Exo2SemiBold, $"[{date.ToShortTimeString()}] {un}", 14)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                X = Avatar.Width + Avatar.X + 5,
                Y = Avatar.Y - 3,
                Tint = Colors.GetUserChatColor(Message.Sender.OnlineUser.UserGroups)
            };
        }

        /// <summary>
        ///    Creates the text that holds the message content.
        /// </summary>
        private void CreateMessageContentText() => TextMessageContent = new SpriteText(Fonts.SourceSansProSemiBold, Message.Message, 14,
                (int)(Container.Width - Avatar.Width - Avatar.X - 5))
        {
            Parent = this,
            UsePreviousSpriteBatchOptions = true,
            X = TextUsername.X,
            Y = TextUsername.Y + TextUsername.Height + 1,
        };

        /// <summary>
        ///     Create the chat badge for the user if they are eligible to have one.
        /// </summary>
        private void CreateChatBadge()
        {
            if (!Message.Sender.IsSpecial)
                return;

            ChatBadge = new ChatBadge(Message.Sender.OnlineUser.UserGroups)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                X = TextUsername.X + TextUsername.Width + 5,
                Y = TextUsername.Y,
            };
        }

        /// <summary>
        ///     Calculates the height of the message.
        /// </summary>
        private void RecalculateHeight()
        {
            Height = 0;
            var maxHeight = Math.Max(Avatar.Height, TextMessageContent.Height);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (maxHeight == Avatar.Height)
                Height = Avatar.Height;
            else
                Height = TextUsername.Height + maxHeight;

            Height += Padding * 2;
        }

        /// <summary>
        ///     Called when a new steam avatar is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSteamAvatarLoaded(object sender, SteamAvatarLoadedEventArgs e)
        {
            // If it doesn't apply to this message.
            if (e.SteamId != (ulong) Message.Sender.OnlineUser.SteamId)
                return;

            try
            {
                Avatar.Animations.Clear();
                Avatar.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 300));
                Avatar.Image = e.Texture;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, LogType.Runtime);
            }
        }
    }
}
