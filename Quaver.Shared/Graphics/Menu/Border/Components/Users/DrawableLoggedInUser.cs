using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using SQLite;
using Steamworks;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Users
{
    public class DrawableLoggedInUser : Sprite, IMenuBorderItem
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UseCustomPaddingY { get; } = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public int CustomPaddingY { get; } = -1;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UseCustomPaddingX { get; } = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public int CustomPaddingX { get; } = 30;

        /// <summary>
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        /// </summary>
        private CircleAvatar Avatar { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Username { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Caret { get; set; }

        /// <summary>
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        ///     Event invoked when the logged in user has been resized
        /// </summary>
        public event EventHandler<LoggedInUserResizedEventArgs> Resized;

        /// <summary>
        /// </summary>
        public DrawableLoggedInUser()
        {
            Height = 54;
            Alpha = 0f;

            CreateButton();
            CreateAvatar();
            CreateUsername();
            CreateCaret();
            UpdateSize();

            if (ConfigManager.Username != null)
                ConfigManager.Username.ValueChanged += OnUsernameChanged;

            OnlineManager.Status.ValueChanged += OnOnlineStatusChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var hoveredColor = SkinManager.Skin?.MenuBorder?.ButtonTextHoveredColor ?? Colors.MainAccent;
            var unhoveredColor = SkinManager.Skin?.MenuBorder?.ButtonTextColor ?? Color.White;

            var color = Button.IsHovered ? hoveredColor : unhoveredColor;
            Caret.FadeToColor(color, gameTime.ElapsedGameTime.TotalMilliseconds, 30);
            Username.Tint = Caret.Tint;

            Button.Size = Size;

            var game = GameBase.Game as QuaverGame;

            if (MouseManager.IsUniqueClick(MouseButton.Left) && !Button.IsHovered
               && game?.CurrentScreen?.ActiveLoggedInUserDropdown != null
               && !game.CurrentScreen.ActiveLoggedInUserDropdown.Button.IsHovered()
               && IsOpen)
            {
               Button?.FireButtonClickEvent();
            }

            if (DialogManager.Dialogs.Count == 0)
            {
                if (KeyboardManager.IsUniqueKeyPress(Keys.F10))
                    Button?.FireButtonClickEvent();
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (ConfigManager.Username != null)
                ConfigManager.Username.ValueChanged -= OnUsernameChanged;

            OnlineManager.Status.ValueChanged -= OnOnlineStatusChanged;
            Resized = null;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
                Alpha = 0,
            };

            Button.Clicked += (sender, args) =>
            {
                IsOpen = !IsOpen;

                AnimateCaret();

                var game = GameBase.Game as QuaverGame;

                if (!IsOpen)
                {
                    game?.CurrentScreen?.ActiveLoggedInUserDropdown?.Close();
                    return;
                }

                if (game?.CurrentScreen?.ActiveLoggedInUserDropdown == null)
                {
                    game?.CurrentScreen?.ActivateLoggedInUserDropdown(new LoggedInUserDropdown(),
                        new ScalableVector2(AbsolutePosition.X + AbsoluteSize.X - LoggedInUserDropdown.ContainerSize.X.Value,
                            AbsolutePosition.Y + AbsoluteSize.Y + 13));

                    return;
                }

                game.CurrentScreen.ActiveLoggedInUserDropdown?.Open();
            };
        }

        /// <summary>
        /// </summary>
        private void CreateAvatar()
        {
            const float scale = 0.60f;

            Avatar = new CircleAvatar(new ScalableVector2(Height * scale, Height * scale), GetAvatar())
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
            };
        }

        /// <summary>
        /// </summary>
        private void CreateUsername()
        {
            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Login", 21)
            {
                Parent = Avatar,
                Alignment = Alignment.MidLeft,
                X =  Avatar.Width + 10,
            };

            UpdateText();
        }

        /// <summary>
        /// </summary>
        private void CreateCaret()
        {
            Caret = new Sprite()
            {
                Parent = this,
                X = -4,
                Alignment = Alignment.MidRight,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_chevron_arrow_down),
                Size = new ScalableVector2(10, 10)
            };
        }

        /// <summary>
        /// </summary>
        private void UpdateText() => ScheduleUpdate(() =>
        {
            Avatar.AvatarSprite.Image = GetAvatar();
            Username.Text = OnlineManager.Connected ? ConfigManager.Username?.Value : "Login";

            UpdateSize();

            Resized?.Invoke(this, new LoggedInUserResizedEventArgs());
        });

        /// <summary>
        /// </summary>
        private void UpdateSize() => Size = new ScalableVector2(Username.Width + Avatar.Width + 36, Avatar.Height);

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUsernameChanged(object sender, BindableValueChangedEventArgs<string> e) => UpdateText();

        /// <summary>
        /// </summary>
        public void AnimateCaret()
        {
            var rotation = IsOpen ? 180 : 0;

            Caret.Animations.Add(new Animation(AnimationProperty.Rotation, Easing.Linear,
                MathHelper.ToDegrees(Caret.Rotation), rotation, 200));
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOnlineStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
            => UpdateText();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private Texture2D GetAvatar()
        {
            var image = UserInterface.OfflineAvatar;

            if (OnlineManager.Status.Value == ConnectionStatus.Connected && SteamManager.UserAvatars != null)
            {
                var id = SteamUser.GetSteamID().m_SteamID;

                if (SteamManager.UserAvatars.ContainsKey(id))
                    image = SteamManager.UserAvatars[id];
            }

            return image;
        }
    }
}