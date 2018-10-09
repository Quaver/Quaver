using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Config;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Menu.UI.Navigation
{
    public class NavbarItemUser : NavbarItem
    {
        /// <summary>
        ///     The user's avatar.
        /// </summary>
        public Sprite Avatar { get; private set; }

        /// <summary>
        ///     The text that shows the user's username
        /// </summary>
        public SpriteTextBitmap UsernameText { get; private set; }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public NavbarItemUser()
        {
            Selected = false;
            UsePreviousSpriteBatchOptions = true;
            Size = new ScalableVector2(175, 45);
            Tint = Color.Black;
            Alpha = 0f;

            CreateAvatar();
            CreateUsername();
            CreateBottomLine();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            if (IsHovered)
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
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateAvatar() => Avatar = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(25, 25),
            Alignment = Alignment.MidLeft,
            Image = UserInterface.UnknownAvatar,
            X = 25
        };

        /// <summary>
        ///     Creates the text for the username.
        /// </summary>
        private void CreateUsername()
        {
            UsernameText = new SpriteTextBitmap(BitmapFonts.Exo2SemiBoldItalic, ConfigManager.Username.Value, 24, Color.White,
                Alignment.MidLeft, int.MaxValue)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Avatar.X + Avatar.Width + 5,
                Y = 2,
                SpriteBatchOptions = new SpriteBatchOptions() { BlendState = BlendState.NonPremultiplied }
            };

            UpdateUsernameSize();

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
            UpdateUsernameSize();
        }

        /// <summary>
        ///     Updates the username size properly.
        /// </summary>
        private void UpdateUsernameSize()
        {
            UsernameText.Size = new ScalableVector2(UsernameText.Width * 0.55f, UsernameText.Height * 0.55f);
            Resize();
        }

        /// <summary>
        ///     Realigns the size of the item.
        /// </summary>
        private void Resize() => Width = Avatar.X + Avatar.Width + UsernameText.Width + Avatar.X + 5;
    }
}