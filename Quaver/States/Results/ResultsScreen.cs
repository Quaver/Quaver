using System;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UserInterface;
using Quaver.Main;
using Quaver.States.Enums;
using Quaver.States.Gameplay;
using Microsoft.Xna.Framework;
using Quaver.States.Select;

namespace Quaver.States.Results
{
    internal class ResultsScreen : IGameState
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public State CurrentState { get; set; } = State.Results;
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     Reference to the gameplay screen that was just played.
        /// </summary>
        private GameplayScreen GameplayScreen { get; }

        /// <summary>
        ///     Container for all sprites.
        /// </summary>
        private Container Container { get; set; }

        /// <summary>
        ///     Transitioner for this screen.
        /// </summary>
        private Sprite ScreenTransitioner { get; set; }

        /// <summary>
        ///     Back to menu button.
        /// </summary>
        private BasicButton Back { get; set; }

        /// <summary>
        ///     If we're currently exiting the screen.
        /// </summary>
        private bool IsExitingScreen { get; set; }

        /// <summary>
        ///     When the user is exiting the screen, this counter will determine when
        ///     to switch to the next screen.
        /// </summary>
        private double TimeSinceExitingScreen { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="gameplay"></param>
        public ResultsScreen(GameplayScreen gameplay) => GameplayScreen = gameplay;
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Initialize()
        {
            Container = new Container();

#region SPRITE_CREATION           
            CreateBackButton();
            
            // Create Screen Transitioner. Draw Last!
            ScreenTransitioner = new Sprite
            {
                Parent = Container,
                Tint = Color.Black,
                Alpha = 1,
                ScaleX = 1,
                ScaleY = 1
            };
#endregion
            UpdateReady = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <exception cref="!:NotImplementedException"></exception>
        public void UnloadContent()
        {
            Container.Destroy();
        }

         /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {            
            Container.Update(dt);
            HandleScreenTransitions(dt);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.Black);
            GameBase.SpriteBatch.Begin();
            
            BackgroundManager.Draw();
            Container.Draw();
            
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Handles all screen tra
        /// </summary>
        /// <param name="dt"></param>
        private void HandleScreenTransitions(double dt)
        {
            // Allow the cursor to be shown again regardless
            GameBase.Cursor.FadeIn(dt, 240);

            // Fade-In
            if (!IsExitingScreen)
            {
                // Fade background back in.        
                BackgroundManager.Readjust();

                ScreenTransitioner.FadeOut(dt, 240);      
            }
            // Exiting Screen
            else
            {
                // Add to the time if the user is exiting the screen in any way.
                TimeSinceExitingScreen += dt;
                
                // Fade BG
                BackgroundManager.Blacken();
                
                // Fade Screen
                ScreenTransitioner.FadeIn(dt, 240);   
                
                // Switch to the song select state after a second.
                if (TimeSinceExitingScreen >= 2000)
                    GameBase.GameStateManager.ChangeState(new SongSelectState());
            }
        }

        /// <summary>
        ///     When the back button is clicked. It should start the screen exiting process.
        /// </summary>
        private void CreateBackButton()
        {
            Back = new BasicButton
            {
                Parent = Container,
                Size = new UDim2D(240, 40),
                Alignment = Alignment.BotLeft,
                Image = GameBase.LoadedSkin.PauseBack
            };

            Back.Clicked += (o, e) =>
            {                      
                IsExitingScreen = true;
                GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundBack);  
            };
        }
    }
}