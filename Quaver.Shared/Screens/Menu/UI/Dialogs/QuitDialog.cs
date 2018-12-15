/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Shared.Screens.Menu.UI.Dialogs
{
    public class QuitDialog : DialogScreen
    {
        /// <summary>
        ///     The box where all the content is contained.
        /// </summary>
        public Sprite ContainingBox { get; set; }

        /// <summary>
        ///     The top line of the containing box.
        /// </summary>
        public Line TopLine { get; set; }

        /// <summary>
        ///     Text that asks the user if they're sure they want to quit.
        /// </summary>
        public SpriteText AreYouSure { get; set; }

        /// <summary>
        ///     The line that divides AreYouSure and the buttons.
        /// </summary>
        public Line DividerLine { get; set; }

        /// <summary>
        ///     The button to exit the game.
        /// </summary>
        public TextButton QuitButton { get; set; }

        /// <summary>
        ///     The button to cancel the dialog.
        /// </summary>
        public TextButton CancelButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public QuitDialog() : base(0.75f)
        {
            Logger.Debug($"Opened QuitDialog", LogType.Runtime);
            CreateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreateContainingBox();
            CreateTopLine();
            CreateAreYouSureText();
            CreateDividerLine();
            CreateQuitButton();
            CreateCancelButton();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            QuitButton.Border.FadeToColor(QuitButton.IsHovered ? Color.White : Color.Crimson, dt, 60);
            QuitButton.Text.FadeToColor(QuitButton.IsHovered ? Color.White : Color.Crimson, dt, 60);
            CancelButton.Border.FadeToColor(CancelButton.IsHovered ? Color.White : Colors.MainAccent, dt, 60);
            CancelButton.Text.FadeToColor(CancelButton.IsHovered ? Color.White : Colors.MainAccent, dt, 60);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Dismiss("KeyboardManager (ESC Button)");
        }

        /// <summary>
        ///     Dismisses the dialog.
        /// </summary>
        private void Dismiss(string via = "")
        {
            Logger.Debug($"Dismissed QuitDialog - {via}", LogType.Runtime);
            DialogManager.Dismiss(this);
        }

        /// <summary>
        ///     Creates the containing box for the dialog.
        /// </summary>
        private void CreateContainingBox() => ContainingBox = new Sprite
        {
            Parent = Container,
            Size = new ScalableVector2(WindowManager.Width, 165),
            Alignment = Alignment.MidCenter,
            Tint = Color.Black,
            Alpha = 0.85f,
        };

        /// <summary>
        ///     Creates the top line on top of the containing box.
        /// </summary>
        private void CreateTopLine() => TopLine = new Line(new Vector2(WindowManager.Width, ContainingBox.AbsolutePosition.Y),
            Colors.MainAccent, 1)
        {
            Parent = ContainingBox,
            Alpha = 0.75f,
            UsePreviousSpriteBatchOptions = true
        };

        /// <summary>
        ///     Creates the text that asks if the user is sure they want to quit.
        /// </summary>
        private void CreateAreYouSureText()
        {
            AreYouSure = new SpriteText(Fonts.Exo2Medium, "Are you sure you want to quit the game?", 24)
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopCenter,
                UsePreviousSpriteBatchOptions = true,
                Y = 25
            };

            AreYouSure.Size = new ScalableVector2(AreYouSure.Width * 0.70f, AreYouSure.Height * 0.70f);
        }

        /// <summary>
        ///     Creates the divider line text.
        /// </summary>
        private void CreateDividerLine()
        {
            DividerLine = new Line(Vector2.Zero, Colors.MainAccent, 1)
            {
                Parent = AreYouSure,
                Alignment = Alignment.TopCenter,
                UsePreviousSpriteBatchOptions = true,
                Y = AreYouSure.Height + 20,
                Alpha = 0.75f
            };

            const int lineWidth = 100;
            DividerLine.EndPosition = new Vector2(DividerLine.AbsolutePosition.X + lineWidth, DividerLine.AbsolutePosition.Y);
            DividerLine.X -= lineWidth;
        }

        /// <summary>
        ///     Creates the button to quit the game.
        /// </summary>
        private void CreateQuitButton()
        {
            QuitButton = new TextButton(UserInterface.BlankBox, Fonts.Exo2Medium,
                "Quit", 14, (o, e) =>
                {
                    Logger.Debug($"Exiting game via QuitDialog", LogType.Runtime);

                    var game = GameBase.Game as QuaverGame;
                    game?.Exit();
                })
            {
                Parent = AreYouSure,
                Y = DividerLine.Y + DividerLine.Height + 25,
                UsePreviousSpriteBatchOptions = true,
                Size = new ScalableVector2(200, 40),
                Tint = Color.Transparent,
                Text =
                {
                    Tint = Color.Crimson
                }
            };

            QuitButton.AddBorder(Color.Crimson, 2);
        }

        /// <summary>
        ///     Creates the button to cancel the dialog.
        /// </summary>
        private void CreateCancelButton()
        {
            CancelButton = new TextButton(UserInterface.BlankBox,
                Fonts.Exo2Medium, "Cancel", 14, (o, e) => Dismiss("Cancel Button"))
            {
                Parent = AreYouSure,
                Y = DividerLine.Y + DividerLine.Height + 25,
                UsePreviousSpriteBatchOptions = true,
                X = QuitButton.X + QuitButton.Width + 25,
                Size = new ScalableVector2(200, 40),
                Tint = Color.Transparent,
                Text =
                {
                    Tint = Colors.MainAccent
                }
            };

            CancelButton.AddBorder(Colors.MainAccent, 2);
        }
    }
}
