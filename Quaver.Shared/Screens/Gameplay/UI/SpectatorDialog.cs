using System;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI
{
    public class SpectatorDialog : Sprite
    {
        private Sprite Icon { get; }

        private Sprite LoadingWheel { get; }

        private SpriteTextBitmap Text { get; }

        private SpectatorClient SpectatorClient { get; }

        public SpectatorDialog(SpectatorClient client)
        {
            SpectatorClient = client;

            Image = UserInterface.WaitingPanel;
            Size = new ScalableVector2(450, 134);
            Alpha = 0;
            SetChildrenAlpha = true;

            Icon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_information_button),
                Y = 18,
                Size = new ScalableVector2(24, 24)
            };

            // ReSharper disable once ObjectCreationAsStatement
            Text = new SpriteTextBitmap(FontsBitmap.AllerRegular, "Waiting for host!")
            {
                Parent = this,
                FontSize = 20,
                Y = Icon.Y + Icon.Height + 10,
                Alignment = Alignment.TopCenter
            };

            LoadingWheel = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(40, 40),
                Image = UserInterface.LoadingWheel,
                Alignment = Alignment.TopCenter,
                Y = Text.Y + Text.Height + 10
            };

            OnlineManager.Client.OnUserStatusReceived += OnClientStatusReceived;
        }

        public override void Update(GameTime gameTime)
        {
            PerformLoadingWheelRotation();
            base.Update(gameTime);
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnUserStatusReceived -= OnClientStatusReceived;
            base.Destroy();
        }

        /// <summary>
        ///     Rotates the loading wheel endlessly
        /// </summary>
        private void PerformLoadingWheelRotation()
        {
            if (LoadingWheel.Animations.Count != 0)
                return;

            var rotation = MathHelper.ToDegrees(LoadingWheel.Rotation);
            LoadingWheel.ClearAnimations();
            LoadingWheel.Animations.Add(new Animation(AnimationProperty.Rotation, Easing.Linear, rotation, rotation + 360, 1000));
        }

        private void OnClientStatusReceived(object sender, UserStatusEventArgs e)
        {
            if (!e.Statuses.ContainsKey(SpectatorClient.Player.OnlineUser.Id))
                return;

            switch (e.Statuses[SpectatorClient.Player.OnlineUser.Id].Status)
            {
                case ClientStatus.Selecting:
                    Text.Text = $"The host is currently selecting a map!";
                    break;
                case ClientStatus.Paused:
                    Text.Text = $"The host is currently paused!";
                    break;
                default:
                    Text.Text = $"Waiting for host!";
                    break;
            }
        }
    }
}