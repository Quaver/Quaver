using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Config;
using Quaver.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Screens.Gameplay.UI
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

        /// <summary>
        ///     The X position of where the buttons are hidden off-screen.
        /// </summary>
        private int ButtonInactivePosX { get; } = -500;

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
                Image = SkinManager.Skin.PauseBackground
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
                X = ButtonInactivePosX,
                Alpha = 1
            };

            Continue.Size = new ScalableVector2(Continue.Image.Width, Continue.Image.Height);

            // Retry Button
            Retry = new ImageButton(SkinManager.Skin.PauseRetry, (o, e) =>
            {
                if (!Screen.IsPaused)
                    return;

                SkinManager.Skin.SoundRetry.CreateChannel().Play();
                ScreenManager.ChangeScreen(new GameplayScreen(Screen.Map, Screen.MapHash, Screen.LocalScores));
            })
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Y = 20,
                X = ButtonInactivePosX,
                Alpha = 1
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
                X = ButtonInactivePosX,
                Alpha = 1
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

           // if (Continue.Transformations.Count > 0)
                //Console.WriteLine(Continue.Transformations[0].Properties);

            base.Update(gameTime);
        }

        /// <summary>
        ///     When the pause menu is activated, it'll fade/translate the interface on-screen.
        /// </summary>
        public void Activate()
        {
            ClearTransformations();

            Background.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, 0, 1, 400));
            Continue.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutExpo, ButtonInactivePosX, GetActivePosX(Continue),  400));
            Retry.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutExpo, ButtonInactivePosX, GetActivePosX(Retry), 400));
            Quit.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutExpo, ButtonInactivePosX, GetActivePosX(Quit), 400));
        }

        /// <summary>
        ///     When the pause menu is deactivated, it'll fade/translate the interface off-screen.
        /// </summary>
        public void Deactivate()
        {
            ClearTransformations();

            Background.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, 1, 0, 400));
            Continue.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutExpo, GetActivePosX(Continue), ButtonInactivePosX, 800));
            Retry.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutExpo, GetActivePosX(Retry), ButtonInactivePosX, 800));
            Quit.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutExpo, GetActivePosX(Quit), ButtonInactivePosX, 800));
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
        ///     Clears all transformations for the pause overlay.
        /// </summary>
        private void ClearTransformations()
        {
            Background.Transformations.Clear();
            Continue.Transformations.Clear();
            Retry.Transformations.Clear();
            Quit.Transformations.Clear();
        }
    }
}
