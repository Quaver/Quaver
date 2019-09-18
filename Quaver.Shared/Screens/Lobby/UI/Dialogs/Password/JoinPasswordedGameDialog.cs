using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Lobby.UI.Dialogs.Joining;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.Lobby.UI.Dialogs.Password
{
    public class JoinPasswordedGameDialog : DialogScreen
    {
        private MultiplayerGame Game { get; }

        /// <summary>
        /// </summary>
        private Textbox PasswordBox { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        public JoinPasswordedGameDialog(MultiplayerGame game) : base(0)
        {
            Game = game;
            FadeTo(0.85f, Easing.Linear, 150);

            CreateContent();
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
        public sealed override void CreateContent()
        {
             var background = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Tint = Color.Black,
                Size = new ScalableVector2(WindowManager.Width, 150),
                Alpha = 0,
                SetChildrenAlpha = true
            };

            background.FadeTo(1, Easing.Linear, 150);

            // ReSharper disable once ObjectCreationAsStatement
            new Wobble.Graphics.Sprites.Sprite
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

            // ReSharper disable once ObjectCreationAsStatement
            var text = new SpriteTextBitmap(FontsBitmap.AllerRegular, "Please enter the password to join this multiplayer game.")
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

            PasswordBox = new Textbox(new ScalableVector2(392, 36), FontManager.GetWobbleFont(Fonts.LatoBlack), 13, "", "Enter Password",
                (pw) =>
                {
                    if (pw.Length == 0)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "You need to enter a valid password");
                        return;
                    }

                    DialogManager.Dismiss(this);
                    DialogManager.Show(new JoiningGameDialog(JoiningGameDialogType.Joining));

                    ThreadScheduler.RunAfter(() => OnlineManager.Client.JoinGame(Game, pw), 800);
                })
            {
                Parent = background,
                Alignment = Alignment.TopCenter,
                Y = text.Y + text.Height + 15,
                Image = UserInterface.SelectSearchBackground,
                Alpha = 0.65f,
                AlwaysFocused = true
            };

            PasswordBox.InputText.Alignment = Alignment.MidLeft;
            PasswordBox.Cursor.Alignment = Alignment.MidLeft;
            PasswordBox.AddBorder(Color.White, 2);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                DialogManager.Dismiss(this);
        }
    }
}