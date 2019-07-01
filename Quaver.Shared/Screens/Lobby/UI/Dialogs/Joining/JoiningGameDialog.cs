using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Window;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Shared.Screens.Lobby.UI.Dialogs.Joining
{
    public class JoiningGameDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        private JoiningGameDialogType Type { get; }

        private Sprite Icon { get; set; }

        private Sprite LoadingWheel { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public JoiningGameDialog(JoiningGameDialogType type) : base(0)
        {
            Type = type;
            FadeTo(0.85f, Easing.Linear, 150);
            CreateContent();

            OnlineManager.Client.OnJoinGameFailed += OnJoinGameFailed;
            OnlineManager.Client.OnJoinedMultiplayerGame += OnJoinedGame;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            var container = new Sprite()
            {
                Parent = this,
                Image = UserInterface.WaitingPanel,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(450, 134),
                Alpha = 1,
                SetChildrenAlpha = true,
            };

            Icon = new Sprite
            {
                Parent = container,
                Alignment = Alignment.TopCenter,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_information_button),
                Y = 18,
                Size = new ScalableVector2(24, 24)
            };

            string str;

            switch (Type)
            {
                case JoiningGameDialogType.Joining:
                    str = "Joining multiplayer game. Please wait...";
                    break;
                case JoiningGameDialogType.Creating:
                    str = "Creating multiplayer game. Please wait...";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // ReSharper disable once ObjectCreationAsStatement
            var text = new SpriteTextBitmap(FontsBitmap.AllerRegular, str)
            {
                Parent = container,
                FontSize = 20,
                Y = Icon.Y + Icon.Height + 10,
                Alignment = Alignment.TopCenter
            };

            LoadingWheel = new Sprite()
            {
                Parent = container,
                Size = new ScalableVector2(40, 40),
                Image = UserInterface.LoadingWheel,
                Alignment = Alignment.TopCenter,
                Y = text.Y + text.Height + 10
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!OnlineManager.Connected)
                DialogManager.Dismiss(this);

            PerformLoadingWheelRotation();
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            OnlineManager.Client.OnJoinGameFailed -= OnJoinGameFailed;
            OnlineManager.Client.OnJoinedMultiplayerGame -= OnJoinedGame;

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
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

        private void OnJoinedGame(object sender, JoinedGameEventArgs e) => DialogManager.Dismiss(this);

        private void OnJoinGameFailed(object sender, JoinGameFailedEventargs e) => DialogManager.Dismiss(this);
    }
}