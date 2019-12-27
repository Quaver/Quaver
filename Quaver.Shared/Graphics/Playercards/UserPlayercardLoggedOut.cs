using System;
using Microsoft.Xna.Framework;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Playercards
{
    public class UserPlayercardLoggedOut : Sprite
    {
        /// <summary>
        /// </summary>
        private Sprite Avatar { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Status { get; set; }

        /// <summary>
        /// </summary>
        public IconButton LoginButton { get; private set; }

        /// <summary>
        /// </summary>
        private LoadingWheel Wheel { get; set; }

        /// <summary>
        /// </summary>
        public UserPlayercardLoggedOut()
        {
            Image = UserInterface.OfflinePlayercardPanel;
            Size = new ScalableVector2(323, 68);

            CreateAvatar();
            CreateUsername();
            CreateLoginButton();
            CreateLoadingWheel();

            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            LoginButton.Visible = OnlineManager.Status.Value == ConnectionStatus.Disconnected;
            LoginButton.IsClickable = LoginButton.Visible;

            Wheel.Visible = !LoginButton.Visible;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateAvatar() => Avatar = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = 14,
            UsePreviousSpriteBatchOptions = true,
            Image = UserInterface.OfflineAvatar,
            Size = new ScalableVector2(40, 40)
        };

        /// <summary>
        /// </summary>
        private void CreateUsername()
        {
            Status = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),"", 20)
            {
                Parent = Avatar,
                X = Avatar.Width + 12,
                Y = -2,
                Tint = ColorHelper.HexToColor("#808080"),
                UsePreviousSpriteBatchOptions = true
            };

            UpdateText();
        }

        /// <summary>
        /// </summary>
        private void CreateLoginButton()
        {
            LoginButton = new IconButton(UserInterface.LoginButtonPlayercard)
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                UsePreviousSpriteBatchOptions = true,
                Size = new ScalableVector2(70, 21),
                Position = new ScalableVector2(-12, -12),
                Visible = false
            };

            LoginButton.Clicked += (sender, args) => OnlineManager.Login();
        }

        /// <summary>
        /// </summary>
        private void CreateLoadingWheel()
        {
            Wheel = new LoadingWheel()
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                UsePreviousSpriteBatchOptions = true,
                Size = new ScalableVector2(24, 24),
                Position = new ScalableVector2(-12, -12)
            };
        }

        private void UpdateText() => ScheduleUpdate(() =>
        {
            switch (OnlineManager.Status.Value)
            {
                case ConnectionStatus.Disconnected:
                    Status.Text = "Disconnected";
                    break;
                case ConnectionStatus.Connecting:
                    Status.Text = "Conecting...";
                    break;
                case ConnectionStatus.Connected:
                    Status.Text = "Connected!";
                    break;
                case ConnectionStatus.Reconnecting:
                    Status.Text = "Reconnecting...";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        });

        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
            => UpdateText();
    }
}