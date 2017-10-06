using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.src.GameState
{
    internal enum State
    {
        MainMenu,
        SongSelect,
        SongLoading,
        PlayScreen,
        PlayPause,
        GameOver,
        ScoreScreen,
        LoadingScreen
    }
    internal interface IGameState
    {

        // Initialize the game settings here      
        void Initialize();

        // Load all content here
        void LoadContent(ContentManager content);

        // Unload any content here
        void UnloadContent();

        // Updates the game
        void Update(GameTime gameTime);

        // Draws the game
        void Draw(SpriteBatch spriteBatch, Vector2 WindowSize);

    }

    internal abstract class GameStateBase : IGameState
    {
        protected GraphicsDevice _graphicsDevice;
        public State _currentState;

        public GameStateBase(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }
        
        //Interface default methods
        public abstract void Initialize();
        public abstract void LoadContent(ContentManager content);
        public abstract void UnloadContent();
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch, Vector2 WindowSize);
    }
}