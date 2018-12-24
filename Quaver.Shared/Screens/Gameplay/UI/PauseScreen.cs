/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
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

            // Continue Button
            Continue = new ImageButton(SkinManager.Skin.PauseContinue, (o, e) =>
            {
                if (!Screen.IsPaused)
                    return;

                Screen.Pause();
            })
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Y = -150,
                X = -SkinManager.Skin.PauseContinue.Width,
                Alpha = 1,
                UsePreviousSpriteBatchOptions = true
            };

            Continue.Size = new ScalableVector2(Continue.Image.Width, Continue.Image.Height);

            // Retry Button
            Retry = new ImageButton(SkinManager.Skin.PauseRetry, (o, e) =>
            {
                if (!Screen.IsPaused)
                    return;

                SkinManager.Skin.SoundRetry.CreateChannel().Play();
                QuaverScreenManager.ChangeScreen(new GameplayScreen(Screen.Map, Screen.MapHash, Screen.LocalScores));
            })
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Y = 20,
                X = -SkinManager.Skin.PauseRetry.Width,
                Alpha = 1,
                UsePreviousSpriteBatchOptions = true
            };

            Retry.Size = new ScalableVector2(Retry.Image.Width, Retry.Image.Height);

            // Quit Button
            Quit = new ImageButton(SkinManager.Skin.PauseBack, (o, e) =>
            {
                if (!Screen.IsPaused)
                    return;

                Screen.IsPaused = false;
                Screen.ForceFail = true;
                Screen.HasQuit = true;

                // Make sure the screen transitioner isn't faded out at all
                var screenView = (GameplayScreenView) Screen.View;
                screenView.Transitioner.Alpha = 0;
            })
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Y = 190,
                X = -SkinManager.Skin.PauseBack.Width,
                Alpha = 1,
                UsePreviousSpriteBatchOptions = true
            };

            Quit.Size = new ScalableVector2(Quit.Image.Width, Quit.Image.Height);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Screen.Failed)
                Visible = false;

            base.Update(gameTime);
        }

        /// <summary>
        ///     When the pause menu is activated, it'll fade/translate the interface on-screen.
        /// </summary>
        public void Activate()
        {
            ClearTransformations();

            Background.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 400));
            Continue.MoveToX(GetActivePosX(Continue), Easing.OutExpo, 400);
            Retry.MoveToX(GetActivePosX(Retry), Easing.OutExpo, 400);
            Quit.MoveToX(GetActivePosX(Quit), Easing.OutExpo, 400);
        }

        /// <summary>
        ///     When the pause menu is deactivated, it'll fade/translate the interface off-screen.
        /// </summary>
        public void Deactivate()
        {
            ClearTransformations();

            Background.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 1, 0, 400));
            Continue.MoveToX(-Continue.Width, Easing.OutExpo, 400);
            Retry.MoveToX(-Retry.Width, Easing.OutExpo, 400);
            Quit.MoveToX(-Quit.Width, Easing.OutExpo, 400);
        }

        /// <summary>
        ///     Gets the X position of when the button is active (Middle of the screen).
        ///     Handled by this method because the size of the button is unknown and skinnable up to the user.
        ///     We use this method to place it in the middle regardless.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static float GetActivePosX(ImageButton button) => WindowManager.Width / 2f - button.Width / 2f;

        /// <summary>
        ///     Clears all Animations for the pause overlay.
        /// </summary>
        private void ClearTransformations()
        {
            Background.Animations.Clear();
            Continue.Animations.Clear();
            Retry.Animations.Clear();
            Quit.Animations.Clear();
        }
    }
}