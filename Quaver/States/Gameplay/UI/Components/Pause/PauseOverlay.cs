using System;
using Microsoft.Xna.Framework;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Main;
using Button = System.Windows.Forms.Button;

namespace Quaver.States.Gameplay.UI.Components
{
    internal class PauseOverlay : Container
    {
        internal GameplayScreen Screen { get; }
        internal Sprite Background { get; }
        internal BasicButton Continue { get; }
        internal BasicButton Retry { get; }
        internal BasicButton Quit { get; }

        internal float GetActivePosX(BasicButton button) => GameBase.WindowRectangle.Width / 2f - button.SizeX / 2f;
        internal int ButtonInactivePosX { get; } = -100;

        internal PauseOverlay(GameplayScreen screen)
        {
            Screen = screen;

            // Background 
            Background = new Sprite()
            {
                Parent = this,
                Size = new UDim2D(GameBase.WindowRectangle.Width, GameBase.WindowRectangle.Height),
                Tint = Color.Aqua,
                Alpha = 0
            };

            // Continue Button
            Continue = new BasicButton()
            {
                Parent = this,
                Image = GameBase.QuaverUserInterface.JudgementOverlay,
                Alignment = Alignment.MidLeft,
                PosY = -150,
                PosX = ButtonInactivePosX,
                Alpha = 0
            };
            
            Continue.Size = new UDim2D(Continue.Image.Width, Continue.Image.Height);
            Continue.Clicked += (o, e) =>
            {
                if (!Screen.IsPaused)
                    return;
                
                Screen.Pause();
            };
            
            // Retry Button
            Retry = new BasicButton()
            {
                Parent = this,
                Image = GameBase.QuaverUserInterface.JudgementOverlay,
                Alignment = Alignment.MidLeft,
                PosY = 0,
                PosX = ButtonInactivePosX,
                Alpha = 0
            };
            
            Retry.Size = new UDim2D(Retry.Image.Width, Retry.Image.Height);
            Retry.Clicked += (o, e) =>
            {
                if (!Screen.IsPaused)
                    return;
                
                GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundRetry);
                GameBase.GameStateManager.ChangeState(new GameplayScreen(Screen.Map, Screen.MapHash));
            };
            
            // Quit Button
            Quit = new BasicButton()
            {
                Parent = this,
                Image = GameBase.QuaverUserInterface.JudgementOverlay,
                Alignment = Alignment.MidLeft,
                PosY = 150,
                PosX = ButtonInactivePosX,
                Alpha = 0
            };
            
            Quit.Size = new UDim2D(Quit.Image.Width, Quit.Image.Height);
            Quit.Clicked += (o, e) =>
            {
                if (!Screen.IsPaused)
                    return;
                
                Screen.IsPaused = false;
                Screen.ForceFail = true;
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {            
            if (Screen.IsPaused && !Screen.IsResumeInProgress)
                Activate(dt);
            else
                Deactivate(dt);

            if (Screen.Failed)
                Visible = false;
            
            base.Update(dt);
        }

        /// <summary>
        ///     When the pause menu is activated, it'll fade/translate the interface on-screen.
        /// </summary>
        /// <param name="dt"></param>
        private void Activate(double dt)
        {
            GameBase.Navbar.PerformShowAnimation(dt);
            
            //Background.FadeIn(dt, Screen.UI.PauseFadeTimeScale);
            Continue.FadeIn(dt, Screen.UI.PauseFadeTimeScale);
            Retry.FadeIn(dt, Screen.UI.PauseFadeTimeScale);
            Quit.FadeIn(dt, Screen.UI.PauseFadeTimeScale);
            
            Continue.Translate(new Vector2(GetActivePosX(Continue), Continue.PosY), dt, Screen.UI.PauseFadeTimeScale);
            Retry.Translate(new Vector2(GetActivePosX(Retry), Retry.PosY), dt, Screen.UI.PauseFadeTimeScale);
            Quit.Translate(new Vector2(GetActivePosX(Quit), Quit.PosY), dt, Screen.UI.PauseFadeTimeScale);
        }

        /// <summary>
        ///     When the pause menu is deactivated, it'll fade/translate the interface off-screen.
        /// </summary>
        /// <param name="dt"></param>
        private void Deactivate(double dt)
        {
            GameBase.Navbar.PerformHideAnimation(dt);
            
            Background.FadeOut(dt, Screen.UI.PauseFadeTimeScale * 2f);
            Continue.FadeOut(dt, Screen.UI.PauseFadeTimeScale * 2f);
            Retry.FadeOut(dt, Screen.UI.PauseFadeTimeScale * 2f);
            Quit.FadeOut(dt, Screen.UI.PauseFadeTimeScale * 2f);
            
            Continue.Translate(new Vector2(ButtonInactivePosX, Continue.PosY), dt, Screen.UI.PauseFadeTimeScale * 2f);
            Retry.Translate(new Vector2(ButtonInactivePosX, Retry.PosY), dt, Screen.UI.PauseFadeTimeScale * 2f);
            Quit.Translate(new Vector2(ButtonInactivePosX, Quit.PosY), dt, Screen.UI.PauseFadeTimeScale * 2f);
        }
    }
}