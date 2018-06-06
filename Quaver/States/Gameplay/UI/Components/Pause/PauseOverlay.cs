using Microsoft.Xna.Framework;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.States.Gameplay.UI.Components.Pause
{
    internal class PauseOverlay : Container
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
        private BasicButton Continue { get; }
        
        /// <summary>
        ///     Retry Button
        /// </summary>
        private BasicButton Retry { get; }

        /// <summary>
        ///     Quit Button
        /// </summary>
        private BasicButton Quit { get; }

        /// <summary>
        ///     The X position of where the buttons are hidden off-screen.
        /// </summary>
        private int ButtonInactivePosX { get; } = -500;

        /// <inheritdoc />
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        internal PauseOverlay(GameplayScreen screen)
        {
            Screen = screen;

            // Background 
            Background = new Sprite()
            {
                Parent = this,
                Size = new UDim2D(GameBase.WindowRectangle.Width, GameBase.WindowRectangle.Height),
                Alpha = 0,
                Image = GameBase.LoadedSkin.PauseBackground
            };

            // Continue Button
            Continue = new BasicButton()
            {
                Parent = this,
                Image = GameBase.LoadedSkin.PauseContinue,
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
                Image = GameBase.LoadedSkin.PauseRetry,
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
                Image = GameBase.LoadedSkin.PauseBack,
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
            
            Background.FadeIn(dt, Screen.UI.PauseFadeTimeScale);
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
        
        /// <summary>
        ///     Gets the X position of when the button is active (Middle of the screen).
        ///     Handled by this method because the size of the button is unknown and skinnable up to the user.
        ///     We use this method to place it in the middle regardless.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        internal static float GetActivePosX(BasicButton button) => GameBase.WindowRectangle.Width / 2f - button.SizeX / 2f;
    }
}