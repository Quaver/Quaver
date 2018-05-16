using Quaver.API.Maps;
using Quaver.GameState;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Graphics.UserInterface;
using Quaver.Main;

namespace Quaver.States.Gameplay.UI
{
    internal class GameplayInterface : IGameStateComponent
    {
        /// <summary>
        ///     Reference to the gameplay screen itself.
        /// </summary>
        private GameplayScreen Screen { get; }
        
        /// <summary>
        ///     Contains general purpose stuff for this screen such as the following:
        ///         - Score/Accuracy Display
        ///         - Leaderboard display
        ///         - Song progress time display
        /// </summary>
        private QuaverContainer Container { get; }

        /// <summary>
        ///     The progress bar for the song time.
        /// </summary>
        internal SongTimeProgressBar SongTimeProgressBar { get; set;  }

        /// <summary>
        ///     Ctor -
        /// </summary>
        internal GameplayInterface(GameplayScreen screen)
        {
            Screen = screen;
            Container = new QuaverContainer();
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            // Initialize the progress bar.
            SongTimeProgressBar = new SongTimeProgressBar(Qua.FindSongLength(GameBase.SelectedMap.Qua), 0, new UDim2D(GameBase.WindowRectangle.Width, 6),
                                                        Container, Alignment.BotLeft);
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnloadContent()
        {
            Container.Destroy();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            // Update the current vaalue of the song time progress bar.
            SongTimeProgressBar.CurrentValue = (float) Screen.AudioTiming.CurrentTime;
            
            Container.Update(dt);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Draw()
        {
            Container.Draw();
        }
    }
}