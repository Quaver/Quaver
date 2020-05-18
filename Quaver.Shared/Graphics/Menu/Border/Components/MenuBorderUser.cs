using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components
{
    public class MenuBorderUser : ImageButton, IMenuBorderItem
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UseCustomPaddingY { get; } = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public int CustomPaddingY { get; } = 0;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UseCustomPaddingX { get; } = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public int CustomPaddingX { get; } = 25;

        /// <summary>
        ///     The online user this represents
        /// </summary>
        private User User { get; }

        /// <summary>
        ///     Displays the avatar the user has
        /// </summary>
        private Sprite Avatar { get; set; }

        /// <summary>
        ///     The amount of x spacing between elements
        /// </summary>
        private const int SpacingX = 10;

        /// <summary>
        ///     Displays the username of the current user
        /// </summary>
        private SpriteTextPlus Username { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MenuBorderUser() : base(WobbleAssets.WhiteBox)
        {
            // If online, use the current online user, otherwise create a new online user
            User = OnlineManager.Self ?? new User(new OnlineUser
            {
                CountryFlag = "XX",
                Username = ConfigManager.Username != null ? ConfigManager.Username.Value : "Player",
                Id = 0,
                SteamId = -1
            }, new List<UserStats>());

            CreateAvatar();
            CreateUsername();
            UpdateUser();

            Alpha = 0;
            Tint = Color.White;

            Hovered += OnHover;
            LeftHover += OnHoverLeft;
        }

        /// <summary>
        ///     Creates the avatar sprite for the user.
        /// </summary>
        private void CreateAvatar()
        {
            Avatar = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(32, 32),
                Image = UserInterface.UnknownAvatar,
                X = 8
            };
        }

        /// <summary>
        ///     Creates the username text
        /// </summary>
        private void CreateUsername()
        {
            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), User.OnlineUser.Username, 24)
            {
                Parent = Avatar,
                Alignment = Alignment.MidLeft,
                X = Avatar.Width + SpacingX,
            };
        }

        /// <summary>
        /// </summary>
        private void UpdateUser()
        {
            Username.Text = User.OnlineUser.Username;

            var steamId = (ulong) User.OnlineUser.SteamId;

            if (SteamManager.UserAvatars.ContainsKey(steamId))
                Avatar.Image = SteamManager.UserAvatars[steamId];
            // Request Steam Avatar
            else
                SteamManager.SendAvatarRetrievalRequest(steamId);

            Size = new ScalableVector2(Avatar.Width + SpacingX + Username.Width + 16, 51);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHover(object sender, EventArgs e)
        {
            SkinManager.Skin?.SoundHover.CreateChannel().Play();

            ClearAnimations();
            FadeTo(0.70f, Easing.OutQuint, 300);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHoverLeft(object sender, EventArgs e)
        {
            ClearAnimations();
            FadeTo(0.50f, Easing.OutQuint, 300);
        }
    }
}