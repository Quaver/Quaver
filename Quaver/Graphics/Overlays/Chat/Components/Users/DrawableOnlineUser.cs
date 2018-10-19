using System;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
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
        ///     The user that this user is for.
        /// </summary>
        public User User { get; }

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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="overlay"></param>
        /// <param name="list"></param>
        /// <param name="user"></param>
        public DrawableOnlineUser(ChatOverlay overlay, OnlineUserList list, User user) : base(UserInterface.BlankBox)
        {
            Overlay = overlay;
            OnlineUserList = list;
            User = user;

            Size = new ScalableVector2(OnlineUserList.Width, 50);
            Tint = Color.White;
            Alpha = 0;
            DestroyIfParentIsNull = false;

            CreateAvatar();
            CreateUsername();
            CreateStatus();
        }

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
                Image = UserInterface.UnknownAvatar,
            };

            var userColor = Colors.GetUserChatColor(User.OnlineUser.UserGroups);
            Avatar.AddBorder(new Color(userColor.R / 2, userColor.G / 2, userColor.B / 2), 2);
        }
        /// <summary>
        ///     Creates the username text for the user
        /// </summary>
        private void CreateUsername()
        {
            var un = User.HasUserInfo ? User.OnlineUser.Username : "Loading...";

            Username = new SpriteTextBitmap(BitmapFonts.Exo2Bold, un, 24, Color.White, Alignment.TopLeft, int.MaxValue)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                X = Avatar.X + Avatar.Width + 5,
                Y = 6,
                Tint = Colors.GetUserChatColor(User.OnlineUser.UserGroups)
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
    }
}