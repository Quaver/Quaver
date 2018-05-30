using Microsoft.Xna.Framework;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
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
                Alignment = Alignment.MidCenter,
                PosY = -150,
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
                Alignment = Alignment.MidCenter,
                PosY = 0,
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
                Alignment = Alignment.MidCenter,
                PosY = 150,
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

        private void Activate(double dt)
        {
            Background.FadeIn(dt, Screen.UI.PauseFadeTimeScale);
            Continue.FadeIn(dt, Screen.UI.PauseFadeTimeScale);
            Retry.FadeIn(dt, Screen.UI.PauseFadeTimeScale);
            Quit.FadeIn(dt, Screen.UI.PauseFadeTimeScale);
            
            GameBase.Navbar.PerformShowAnimation(dt);
        }

        private void Deactivate(double dt)
        {
            Background.FadeOut(dt, Screen.UI.PauseFadeTimeScale * 2f);
            Continue.FadeOut(dt, Screen.UI.PauseFadeTimeScale * 2f);
            Retry.FadeOut(dt, Screen.UI.PauseFadeTimeScale * 2f);
            Quit.FadeOut(dt, Screen.UI.PauseFadeTimeScale * 2f);
            
            GameBase.Navbar.PerformHideAnimation(dt);
        }
    }
}