using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Online;
using Quaver.Online.Chat;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Graphics.Overlays.Chat.Components.Users
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
        public SpriteTextBitmap Username { get; private set; }

        /// <summary>
        ///     The user's status
        /// </summary>
        public SpriteTextBitmap Status { get; private set; }

        /// <summary>
        ///     The height of the drawable user.
        /// </summary>
        public static int HEIGHT { get; } = 50;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="overlay"></param>
        /// <param name="list"></param>
        /// <param name="user"></param>
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
                UsePreviousSpriteBatchOptions = true,
                X = 10,
                Alignment = Alignment.MidLeft,
                Image = GetAvatarOrRequest()
            };

            Avatar.AddBorder(Color.White, 2);
        }
        /// <summary>
        ///     Creates the username text
        /// </summary>
        private void CreateUsername()
        {
            Username = new SpriteTextBitmap(BitmapFonts.Exo2Bold, "Loading...", 24, Color.White, Alignment.TopLeft, int.MaxValue)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                X = Avatar.X + Avatar.Width + 5,
                Y = 6,
                Tint = Color.White
            };

            Username.Size = new ScalableVector2(Username.Width * 0.55f, Username.Height * 0.55f);
        }

        /// <summary>
        ///     Creates the text for the user's status
        /// </summary>
        private void CreateStatus()
        {
            Status = new SpriteTextBitmap(BitmapFonts.Exo2SemiBold, "Idle", 24, Color.White, Alignment.TopLeft, int.MaxValue)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                X = Username.X,
                Y = Username.Y + Username.Height - 5
            };

            Status.Size = new ScalableVector2(Status.Width * 0.50f, Status.Height * 0.50f);
        }

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
                Username.Size = new ScalableVector2(Username.Width * 0.55f, Username.Height * 0.55f);

                Username.Tint = Colors.GetUserChatColor(User.OnlineUser.UserGroups);

                SetAvatar(GetAvatarOrRequest());
                Avatar.Border.Tint = Colors.GetUserChatColor(User.OnlineUser.UserGroups);
            }
            else
            {
                // If we don't have user information, request it to the server to obtain it.
                OnlineManager.Client.RequestUserInfo(new List<int>() { User.OnlineUser.Id });

                Username.Text = $"User#{User.OnlineUser.Id}";
                Username.Size = new ScalableVector2(Username.Width * 0.55f, Username.Height * 0.55f);

                Avatar.Border.Tint = Color.White;
                Username.Tint = Color.White;
            }
        }

        /// <summary>
        ///     Updates the user client status text
        /// </summary>
        /// <param name="status"></param>
        public void UpdateStatus(UserClientStatus status)
        {
            var statusText = "Idle";

            switch (status.Status)
            {
                case ClientStatus.InMenus:
                    statusText = "In the menus";
                    break;
                case ClientStatus.Selecting:
                    statusText = "Selecting a song";
                    break;
                case ClientStatus.Playing:
                    statusText = $"Playing {status.Content}";
                    break;
                case ClientStatus.Paused:
                    statusText = $"Paused";
                    break;
                case ClientStatus.Watching:
                    statusText = $"Watching {status.Content}";
                    break;
                case ClientStatus.Editing:
                    statusText = $"Editing: {status.Content}";
                    break;
            }

            Status.Text = statusText;
            Status.Size = new ScalableVector2(Status.Width * 0.50f, Status.Height * 0.50f);
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
                    Console.WriteLine(steamId);

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
        private void SetAvatar(Texture2D tex)
        {
            lock (Avatar.Transformations)
            {
                Avatar.Transformations.Clear();
                Avatar.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, 0, 1, 300));
                Avatar.Image = tex;
            }
        }
    }
}