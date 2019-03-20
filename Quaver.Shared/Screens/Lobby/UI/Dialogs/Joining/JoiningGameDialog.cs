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
            var background = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Tint = Color.Black,
                Size = new ScalableVector2(WindowManager.Width, 100),
                Alpha = 0,
                SetChildrenAlpha = true
            };

            background.FadeTo(1, Easing.Linear, 150);

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = background,
                Size = new ScalableVector2(Width, 2),
                Tint = Colors.MainAccent,
                Alpha = 0
            };

            var icon = new Sprite
            {
                Parent = background,
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
                    str = "Creating multiplyer game. Please wait...";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // ReSharper disable once ObjectCreationAsStatement
            var text = new SpriteTextBitmap(FontsBitmap.AllerRegular, str)
            {
                Parent = background,
                FontSize = 20,
                Y = icon.Y + icon.Height + 15,
                Alignment = Alignment.TopCenter,
                Alpha = 1
            };

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = background,
                Size = new ScalableVector2(Width, 2),
                Tint = Colors.MainAccent,
                Alignment = Alignment.BotLeft
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

        private void OnJoinedGame(object sender, JoinedGameEventArgs e) => DialogManager.Dismiss(this);

        private void OnJoinGameFailed(object sender, JoinGameFailedEventargs e) => DialogManager.Dismiss(this);
    }
}