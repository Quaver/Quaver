/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.Metadata
{
    public class EditorMetadataConfirmationDialog : DialogScreen
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
        ///     Text that asks the user if they're sure.
        /// </summary>
        public SpriteText AreYouSure { get; set; }

        /// <summary>
        ///     The line that divides AreYouSure and the buttons.
        /// </summary>
        public Line DividerLine { get; set; }

        /// <summary>
        ///     The button to confirm
        /// </summary>
        public TextButton SureButton { get; set; }

        /// <summary>
        ///     The button to cancel the dialog.
        /// </summary>
        public TextButton CancelButton { get; set; }

        /// <summary>
        /// </summary>
        private EditorScreen Screen { get; }

        /// <summary>
        ///     The background file to use.
        /// </summary>
        private EditorMetadataChanger Changer { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorMetadataConfirmationDialog(EditorScreen screen, EditorMetadataChanger changer) : base(0)
        {
            Screen = screen;
            Changer = changer;
            Screen.InBackgroundConfirmationDialog = true;

            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 0.75f, 100));
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
            CreateSureButton();
            CreateCancelButton();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            SureButton.Border.FadeToColor(SureButton.IsHovered ? Color.White : Color.LimeGreen, dt, 60);
            SureButton.Text.FadeToColor(SureButton.IsHovered ? Color.White : Color.LimeGreen, dt, 60);
            CancelButton.Border.FadeToColor(CancelButton.IsHovered ? Color.White : Color.Crimson, dt, 60);
            CancelButton.Text.FadeToColor(CancelButton.IsHovered ? Color.White : Color.Crimson, dt, 60);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Dismiss();
        }

        /// <summary>
        ///     Dismisses the dialog.
        /// </summary>
        private void Dismiss()
        {
            Screen.InBackgroundConfirmationDialog = false;
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
        /// </summary>
        private void CreateAreYouSureText()
        {
            AreYouSure = new SpriteText(Fonts.Exo2Medium, "Would you like to save your changes?", 24)
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
        private void CreateSureButton()
        {
            SureButton = new TextButton(UserInterface.BlankBox, Fonts.Exo2Medium,
                "Sure", 14, (o, e) =>
                {
                    var modeBeforeSave = Screen.WorkingMap.Mode;
                    Changer.Container.Items.ForEach(x => x.SaveValue(x.GetValue()));

                    // Need to handle the game mode change.
                    if (modeBeforeSave != Screen.WorkingMap.Mode)
                    {
                        Logger.Important($"Game mode changed from: {modeBeforeSave} to {Screen.WorkingMap.Mode}", LogType.Runtime);

                        // Mode changed from 7k to 4k, so get rid of all the 4k HitObjects
                        if (modeBeforeSave == GameMode.Keys7)
                            Screen.WorkingMap.HitObjects.RemoveAll(x => x.Lane > 4);

                        // Need to destroy/remake the scroll container.
                        var ruleset = Screen.Ruleset as EditorRulesetKeys;
                        ruleset?.ScrollContainer.Destroy();
                        ruleset?.CreateScrollContainer();
                    }

                    Screen.Save();
                    Dismiss();
                    Changer.Dialog.Close();
                })
            {
                Parent = AreYouSure,
                Y = DividerLine.Y + DividerLine.Height + 25,
                UsePreviousSpriteBatchOptions = true,
                Size = new ScalableVector2(200, 40),
                Tint = Color.Transparent,
                Text =
                {
                    Tint = Color.LimeGreen
                }
            };

            SureButton.AddBorder(Color.LimeGreen, 2);
        }

        /// <summary>
        ///     Creates the button to cancel the dialog.
        /// </summary>
        private void CreateCancelButton()
        {
            CancelButton = new TextButton(UserInterface.BlankBox,
                Fonts.Exo2Medium, "Cancel", 14, (o, e) => Dismiss())
            {
                Parent = AreYouSure,
                Y = DividerLine.Y + DividerLine.Height + 25,
                UsePreviousSpriteBatchOptions = true,
                X = SureButton.X + SureButton.Width + 25,
                Size = new ScalableVector2(200, 40),
                Tint = Color.Transparent,
                Text =
                {
                    Tint = Color.Crimson
                }
            };

            CancelButton.AddBorder(Color.Crimson, 2);
        }
    }
}
