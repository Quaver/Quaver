/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Graphics.Overlays.Chat.Components.Users
{
    public class DrawableOnlineUser : ImageButton
    {
        /// <summary>
        ///     Reference to the parent chat overlay.
        /// </summary>
        public ChatOverlay Overlay { get; }

        /// <summary>
        ///     Reference to the parent online user list.
        /// </summary>
        public OnlineUserList OnlineUserList { get; }

        /// <summary>
        ///     The user that this is referencing.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        ///     The user's avatar.
        /// </summary>
        public Sprite Avatar { get; private set; }

        /// <summary>
        ///     The user's username
        /// </summary>
        public SpriteText Username { get; private set; }

        /// <summary>
        ///     The user's status
        /// </summary>
        public SpriteText Status { get; private set; }

        /// <summary>
        ///     The height of the drawable user.
        /// </summary>
        public static int HEIGHT { get; } = 50;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="overlay"></param>
        /// <param name="list"></param>
        public DrawableOnlineUser(ChatOverlay overlay, OnlineUserList list) : base(UserInterface.BlankBox)
        {
            Overlay = overlay;
            OnlineUserList = list;
            Size = new ScalableVector2(OnlineUserList.Width, HEIGHT);
            Tint = Color.White;
            Alpha = 0;
            DestroyIfParentIsNull = false;

            SteamManager.SteamUserAvatarLoaded += OnSteamAvatarLoaded;

            CreateAvatar();
            CreateUsername();
            CreateStatus();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            var targetAlpha = IsHovered ? 0.45f : 0;
            Alpha = MathHelper.Lerp(Alpha, targetAlpha, (float) Math.Min(dt / 60, 1));

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            SteamManager.SteamUserAvatarLoaded -= OnSteamAvatarLoaded;

            base.Destroy();
        }

        /// <summary>
        ///     Creates the avatar for the user.
        /// </summary>
        private void CreateAvatar()
        {
            Avatar = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(Height * 0.80f, Height * 0.80f),
                X = 10,
                Alignment = Alignment.MidLeft,
                Image = GetAvatarOrRequest()
            };

            Avatar.AddBorder(Color.White, 2);
        }
        /// <summary>
        ///     Creates the username text
        /// </summary>
        private void CreateUsername() => Username = new SpriteText(Fonts.Exo2Bold, "Loading...", 13)
        {
            Parent = this,
            X = Avatar.X + Avatar.Width + 5,
            Y = 6,
            Tint = Color.White
        };

        /// <summary>
        ///     Creates the text for the user's status
        /// </summary>
        private void CreateStatus() => Status = new SpriteText(Fonts.SourceSansProSemiBold, "Idle", 12)
        {
            Parent = this,
            X = Username.X,
            Y = Username.Y + Username.Height - 2
        };

        /// <summary>
        ///     Updates the drawable with new user information.
        /// </summary>
        /// <param name="user"></param>
        public void UpdateUser(User user)
        {
            User = user;

            if (User.HasUserInfo)
            {
                Username.Text = User.OnlineUser.Username;
                Username.Tint = Colors.GetUserChatColor(User.OnlineUser.UserGroups);

                SetAvatar(GetAvatarOrRequest());
                Avatar.Border.Tint = Colors.GetUserChatColor(User.OnlineUser.UserGroups);
            }
            else
            {
                // If we don't have user information, request it to the server to obtain it.
                if (ChatManager.IsActive)
                    OnlineManager.Client.RequestUserInfo(new List<int>() { User.OnlineUser.Id });

                Username.Text = $"User#{User.OnlineUser.Id}";
                Avatar.Border.Tint = Color.White;
                Username.Tint = Color.White;
            }

            UpdateStatus();
        }

        /// <summary>
        ///     Updates the user client status text
        /// </summary>
        public void UpdateStatus()
        {
            var statusText = "Idle";

            switch (User.CurrentStatus.Status)
            {
                case ClientStatus.InMenus:
                    statusText = "Idle";
                    break;
                case ClientStatus.Selecting:
                    statusText = "Selecting a Song";
                    break;
                case ClientStatus.Playing:
                    statusText = $"Playing";
                    break;
                case ClientStatus.Paused:
                    statusText = $"Paused";
                    break;
                case ClientStatus.Watching:
                    statusText = $"Watching {User.CurrentStatus.Content}";
                    break;
                case ClientStatus.Editing:
                    statusText = $"Editing a Map";
                    break;
            }

            Status.Text = statusText;
        }

        /// <summary>
        ///     Called when a user's steam avatar is loaded. Updates the user's avatar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnSteamAvatarLoaded(object sender, SteamAvatarLoadedEventArgs e)
        {
            if (User == null || !User.HasUserInfo || e.SteamId != (ulong) User.OnlineUser.SteamId)
                return;

            SetAvatar(e.Texture);
        }

        /// <summary>
        ///     Retrieves the avatar to use for the user.
        ///
        ///     If no avatar is available then it'll request to Steam to retrieve it.
        /// </summary>
        /// <returns></returns>
        private Texture2D GetAvatarOrRequest()
        {
            var avatar = UserInterface.UnknownAvatar;

            // If we've got a Steam avatar cached, then use it, if not, request it.
            if (User != null)
            {
                if (User.HasUserInfo)
                {
                    var steamId = (ulong) User.OnlineUser.SteamId;

                    if (SteamManager.UserAvatars.ContainsKey(steamId))
                        avatar = SteamManager.UserAvatars[steamId];
                    else
                        SteamManager.SendAvatarRetrievalRequest(steamId);
                }
            }

            return avatar;
        }

        /// <summary>
        ///     Sets the user's avatar and does an animation.
        /// </summary>
        /// <param name="tex"></param>
        private void SetAvatar(Texture2D tex) => Avatar.Image = tex;
    }
}
