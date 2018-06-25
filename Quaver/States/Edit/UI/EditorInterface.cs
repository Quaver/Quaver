using Microsoft.Xna.Framework;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.UserInterface;
using Quaver.Main;
using Quaver.States.Edit.UI.Components;

namespace Quaver.States.Edit.UI
{
    internal class EditorInterface : IGameStateComponent
    {
        /// <summary>
        ///     Reference to the parent editor screen.
        /// </summary>
        private EditorScreen Screen { get; }

        /// <summary>
        ///     Sprite container for all editor elements.
        /// </summary>
        private Container Container { get; set; }

        /// <summary>
        ///     The current time in the map.
        /// </summary>
        private EditorSongTimeDisplay CurrentTime { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        internal EditorInterface(EditorScreen screen) => Screen = screen;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            Container = new Container();
            
            CreateSongTimeDisplay();
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
            Container.Update(dt);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            
            BackgroundManager.Draw();
            Container.Draw();
            
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Creates the 
        /// </summary>
        private void CreateSongTimeDisplay()
        {
            CurrentTime = new EditorSongTimeDisplay(NumberDisplayType.SongTime, "00:00", new Vector2(2, 2))
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,     
                PosY = -30,
                PosX = 0
            };
        }
    }
}