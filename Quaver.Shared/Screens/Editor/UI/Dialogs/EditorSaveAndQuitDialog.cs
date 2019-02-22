/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Dialogs;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs
{
    public class EditorSaveAndQuitDialog : ConfirmCancelDialog
    {
        /// <summary>
        /// </summary>
        private EditorScreen Screen { get; }

        /// <summary>
        ///     The button to not save and exit.
        /// </summary>
        public TextButton NoButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorSaveAndQuitDialog(EditorScreen screen): base("You have unsaved changes. Would you like to save before quitting?", null)
        {
            Screen = screen;
            OnConfirm += (sender, args) => Screen.Save(true);
            CreateNoButton();
            AlignButtons();
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            NoButton.Border.FadeToColor(NoButton.IsHovered ? Color.White : Colors.MainAccent, dt, 60);
            NoButton.Text.FadeToColor(NoButton.IsHovered ? Color.White : Colors.MainAccent, dt, 60);

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateNoButton()
        {
            NoButton = new TextButton(UserInterface.BlankBox, Fonts.Exo2Medium,
                "No", 14, (o, e) =>
                {
                    DialogManager.Dismiss(this);
                    Screen.ExitToSelect();
                })
            {
                Parent = DividerLine,
                Y = 20,
                UsePreviousSpriteBatchOptions = true,
                Size = new ScalableVector2(200, 40),
                Tint = Color.Transparent,
                Text =
                {
                    Tint = Colors.MainAccent
                }
            };

            NoButton.X = -NoButton.Width / 2f - 10;
            NoButton.AddBorder(Color.Gold, 2);
        }

        /// <summary>
        /// </summary>
        private void AlignButtons()
        {
            NoButton.Parent = ContainingBox;
            NoButton.Alignment = Alignment.TopCenter;
            NoButton.X = 0;
            NoButton.Y = AreYouSure.Y + AreYouSure.Height + 40;

            SureButton.Parent = NoButton;
            SureButton.Alignment = Alignment.TopLeft;
            SureButton.X = -NoButton.Width - 15;
            SureButton.Y = 0;

            CancelButton.Parent = NoButton;
            CancelButton.Alignment = Alignment.TopLeft;
            CancelButton.X = NoButton.Width + 15;
            CancelButton.Y = 0;
        }
    }
}