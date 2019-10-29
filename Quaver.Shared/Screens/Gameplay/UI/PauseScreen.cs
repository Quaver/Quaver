/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.Shared.Config;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Skinning;
using System;
using System.Collections.Generic;
using Quaver.Shared.Online;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Window;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Shared.Screens.Gameplay.UI
{
    public class PauseScreen : Container
    {
        /// <summary>
        ///     Reference to the gameplay screen itself
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <summary>
        ///     Reference to the container used for buttons
        /// </summary>
        private Container Container { get; }

        /// <summary>
        ///     The pause overlay's background
        ///     (Skinnable)
        /// </summary>
        private Sprite Background { get; }

        /// <summary>
        ///     Continue Button
        /// </summary>
        private ImageButton Continue { get; }

        /// <summary>
        ///     Retry Button
        /// </summary>
        private ImageButton Retry { get; }

        /// <summary>
        ///     Quit Button
        /// </summary>
        private ImageButton Quit { get; }

        /// <summary>
        ///     Current Button selected
        /// </summary>
        private ImageButton Selected { get; set; }

        /// <summary>
        ///     Current Selected index for selected button
        /// </summary>
        private int SelectedIndex { get; set; }

        /// <summary>
        ///     List of Buttons in the Pause Screen
        /// </summary>
        private List<ImageButton> Buttons { get; } = new List<ImageButton>();

        /// <summary>
        ///     Color used for tinted buttons (when they aren't hovered over)
        /// </summary>
        private Color TintColor { get; } = Color.DarkGray;

        /// <summary>
        ///     Animation Time for Tint when hovering over button
        /// </summary>
        private const int ANIMATION_TIME = 400;

        /// <summary>
        ///     Position of Button Container when Pause Screen is inactive
        /// </summary>
        private const int INACTIVE_X_POSITION = -100;

        /// <summary>
        ///     Position of active button when it is hovered over
        /// </summary>
        private const int HOVER_X_OFFSET = -10;

        /// <inheritdoc />
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        public PauseScreen(GameplayScreen screen)
        {
            Screen = screen;

            // Background
            Background = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                Alpha = 0,
                Image = SkinManager.Skin.PauseBackground,
            };

            // Container
            Container = new Container()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                X = INACTIVE_X_POSITION
            };

            // Continue Button
            Continue = new ImageButton(SkinManager.Skin.PauseContinue, (o, e) => InitiateContinue())
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                Y = -150,
                Alpha = 0,
                UsePreviousSpriteBatchOptions = true
            };
            Buttons.Add(Continue);
            Continue.Size = new ScalableVector2(Continue.Image.Width, Continue.Image.Height);
            Continue.Hovered += (o, e) => HoverButton(0);

            // Retry Button
            Retry = new ImageButton(SkinManager.Skin.PauseRetry, (o, e) => InitiateRetry())
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                Y = 20,
                Alpha = 0,
                Tint = TintColor,
                UsePreviousSpriteBatchOptions = true
            };
            Buttons.Add(Retry);
            Retry.Size = new ScalableVector2(Retry.Image.Width, Retry.Image.Height);
            Retry.Hovered += (o, e) => HoverButton(1);

            // Quit Button
            Quit = new ImageButton(SkinManager.Skin.PauseBack, (o, e) => InitiateQuit())
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                Y = 190,
                Alpha = 0,
                Tint = TintColor,
                UsePreviousSpriteBatchOptions = true
            };
            Buttons.Add(Quit);
            Quit.Size = new ScalableVector2(Quit.Image.Width, Quit.Image.Height);
            Quit.Hovered += (o, e) => HoverButton(2);

            // Select continue button on initialization
            HoverButton(0, true);
        }

        /// <summary>
        ///     Handle Up Navigation Button
        /// </summary>
        private void HandleKeyPressUp()
        {
            if (!KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyNavigateUp.Value))
                return;

            var index = SelectedIndex == 0 ? Buttons.Count - 1 : SelectedIndex - 1;
            HoverButton(index);
        }

        /// <summary>
        ///     Handle Down Navigation Button
        /// </summary>
        private void HandleKeyPressDown()
        {
            if (!KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyNavigateDown.Value))
                return;

            var index = SelectedIndex == Buttons.Count - 1 ? 0 : SelectedIndex + 1;
            HoverButton(index);
        }

        /// <summary>
        ///     Handle Select Button
        /// </summary>
        private void HandleKeyPressSelect()
        {
            if (!KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyNavigateSelect.Value))
                return;

            if (Selected == Continue)
                InitiateContinue();

            else if (Selected == Retry)
                InitiateRetry();

            else if (Selected == Quit)
                InitiateQuit();
        }

        /// <summary>
        ///     Hover over a specific button via given index.
        /// </summary>
        /// <param name="button"></param>
        private void HoverButton(int index, bool dontPlayAudio = false)
        {
            SelectedIndex = index;
            Selected = Buttons[index];

            // Update Animations
            Buttons.ForEach(x => ClearNonAlphaAnimations(x));
            foreach (var button in Buttons)
            {
                ClearNonAlphaAnimations(button);

                if (button == Selected)
                {
                    button.FadeToColor(Color.White, Easing.OutQuint, ANIMATION_TIME);
                    button.MoveToX(GetActivePosX(button) + HOVER_X_OFFSET, Easing.OutQuint, ANIMATION_TIME);

                    continue;
                }

                button.FadeToColor(TintColor, Easing.OutQuint, ANIMATION_TIME);
                button.MoveToX(GetActivePosX(button), Easing.OutQuint, ANIMATION_TIME);
            }
        }

        /// <summary>
        ///     Called when Continue Button gets selected
        /// </summary>
        private void InitiateContinue()
        {
            if (!Screen.IsPaused)
                return;

            Screen.Pause();
        }

        /// <summary>
        ///     Called when Retry Button gets selected
        /// </summary>
        private void InitiateRetry()
        {
            if (!Screen.IsPaused)
                return;

            Screen.Retry();
        }

        /// <summary>
        ///     Called when Quit Buttons gets selected
        /// </summary>
        private void InitiateQuit()
        {
            if (!Screen.IsPaused)
                return;

            Screen.IsPaused = false;
            Screen.ForceFail = true;
            Screen.HasQuit = true;

            // Make sure the screen transitioner isn't faded out at all
            var screenView = (GameplayScreenView)Screen.View;
            screenView.Transitioner.Alpha = 0;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Screen.Failed || Screen.SpectatorClient != null)
                Visible = false;

            if (Screen.IsPaused && DialogManager.Dialogs.Count == 0 && Screen.SpectatorClient == null)
            {
                HandleKeyPressDown();
                HandleKeyPressUp();
                HandleKeyPressSelect();
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     When the pause menu is activated, it'll fade/translate the interface on-screen.
        /// </summary>
        public void Activate()
        {
            // Pausing should never be activated when spectating
            if (Screen.SpectatorClient != null)
                return;

            Background.ClearAnimations();
            Background.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Background.Alpha, 1, ANIMATION_TIME));
            Container.ClearAnimations();
            Container.MoveToX(0, Easing.OutQuint, ANIMATION_TIME);
            Buttons.ForEach(x => x.ClearAnimations());
            Buttons.ForEach(x => x.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint, x.Alpha, 1, ANIMATION_TIME)));
        }

        /// <summary>
        ///     When the pause menu is deactivated, it'll fade/translate the interface off-screen.
        /// </summary>
        public void Deactivate()
        {
            // Pausing should never be activated when spectating
            if (Screen.SpectatorClient != null)
                return;

            Background.ClearAnimations();
            Background.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Background.Alpha, 0, ANIMATION_TIME));
            Container.ClearAnimations();
            Container.MoveToX(INACTIVE_X_POSITION, Easing.OutQuint, ANIMATION_TIME);
            Buttons.ForEach(x => x.ClearAnimations());
            Buttons.ForEach(x => x.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint, x.Alpha, 0, ANIMATION_TIME)));
        }

        /// <summary>
        ///     Clears Non-Alpha Animations for a specific Drawable
        /// </summary>
        /// <param name="drawable"></param>
        private void ClearNonAlphaAnimations(Drawable drawable)
        {
            for (var i = drawable.Animations.Count - 1; i >= 0; i--)
            {
                if (drawable.Animations[i].Properties == AnimationProperty.Alpha)
                    continue;

                drawable.Animations.RemoveAt(i);
            }
        }

        /// <summary>
        ///     Gets the X position of when the button is active (Middle of the screen).
        ///     Handled by this method because the size of the button is unknown and skinnable up to the user.
        ///     We use this method to place it in the middle regardless.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static float GetActivePosX(ImageButton button) => WindowManager.Width / 2f - button.Width / 2f;
    }
}
