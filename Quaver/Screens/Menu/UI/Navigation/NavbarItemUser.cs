using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Online;
using Steamworks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Animations;
using Wobble.Logging;

namespace Quaver.Screens.Menu.UI.Navigation
{
    public class NavbarItemUser : NavbarItem
    {
        /// <summary>
        ///     Reference to the parent menu screen view.
        /// </summary>
        public MenuScreenView View { get; }

        /// <summary>
        ///     The user's avatar.
        /// </summary>
        public Sprite Avatar { get; private set; }

        /// <summary>
        ///     The text that shows the user's username
        /// </summary>
        public SpriteText UsernameText { get; private set; }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public NavbarItemUser(MenuScreenView view)
        {
            View = view;

            Selected = false;
            UsePreviousSpriteBatchOptions = true;
            Size = new ScalableVector2(175, 45);
            Tint = Color.Black;
            Alpha = 0f;

            CreateAvatar();
            SteamManager.SteamUserAvatarLoaded += OnSteamAvatarLoaded;

            CreateUsername();
            CreateBottomLine();

            Clicked += OnClick;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            if (IsHovered || Selected)
                Alpha = MathHelper.Lerp(Alpha, 0.25f, (float) Math.Min(dt / 60, 1));
            else
                Alpha = MathHelper.Lerp(Alpha, 0f, (float) Math.Min(dt / 60, 1));

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            ConfigManager.Username.ValueChanged -= OnConfigUsernameChanged;

            // ReSharper disable once DelegateSubtraction
            SteamManager.SteamUserAvatarLoaded -= OnSteamAvatarLoaded;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateAvatar()
        {
            var userAvatar = UserInterface.UnknownAvatar;

            if (SteamManager.UserAvatars.ContainsKey(SteamUser.GetSteamID().m_SteamID))
                userAvatar = SteamManager.UserAvatars[SteamUser.GetSteamID().m_SteamID];
            else
                SteamManager.SendAvatarRetrievalRequest(SteamUser.GetSteamID().m_SteamID);

            Avatar = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(25, 25),
                Alignment = Alignment.MidLeft,
                Image = userAvatar,
                X = 25,
                SpriteBatchOptions = new SpriteBatchOptions()
                {
                    BlendState = BlendState.NonPremultiplied
                }
            };
        }

        /// <summary>
        ///     Updates the user's avatar.
        /// </summary>
        private void OnSteamAvatarLoaded(object sender, SteamAvatarLoadedEventArgs e)
        {
            if (e.SteamId != SteamUser.GetSteamID().m_SteamID)
                return;

            Avatar.Animations.Clear();
            Avatar.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 300));
            Avatar.Image = e.Texture;
        }

        /// <summary>
        ///     Creates the text for the username.
        /// </summary>
        private void CreateUsername()
        {
            var username = !string.IsNullOrEmpty(ConfigManager.Username.Value) ? ConfigManager.Username.Value : "Player";

            UsernameText = new SpriteText(BitmapFonts.Exo2SemiBold, username, 13)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Avatar.X + Avatar.Width + 5,
            };

            Resize();
            ConfigManager.Username.ValueChanged += OnConfigUsernameChanged;
        }

        /// <summary>
        ///     Called when the user's username changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConfigUsernameChanged(object sender, BindableValueChangedEventArgs<string> e)
        {
            UsernameText.Text = e.Value;
            Resize();

            var parent = Parent;

            while (parent != null && parent.GetType() != typeof(Navbar))
            {
                parent = parent.Parent;

                if (parent.GetType() != typeof(Navbar))
                    continue;

                var navbar = parent as Navbar;
                navbar?.AlignRightItems();
            }
        }

        /// <summary>
        ///     Realigns the size of the item.
        /// </summary>
        private void Resize() => Width = Avatar.X + Avatar.Width + UsernameText.Width + Avatar.X + 5;

        /// <summary>
        ///     Called when the button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClick(object sender, EventArgs e)
        {
            Selected = !Selected;
            View.UserProfile?.PerformClickAnimation(Selected);
        }
    }
}