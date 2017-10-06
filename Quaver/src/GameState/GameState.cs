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
        void Draw(SpriteBatch spriteBatch);

    }

    internal abstract class GameStateBase : IGameState
    {
        protected GraphicsDevice _graphicsDevice;

        //The State of the GameState class. Will be declared when Instantiated.
        private State _currentState;
        public State CurrentState
        {
            get { return _currentState; }
            set { _currentState = value; }
        }

        public GameStateBase(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        // Initialize the game settings here  
        public abstract void Initialize();

        // Load all content here
        public abstract void LoadContent(ContentManager content);

        // Unload any content here
        public abstract void UnloadContent();

        // Updates the game
        public abstract void Update(GameTime gameTime);

        // Draws the game
        public abstract void Draw(SpriteBatch spriteBatch);
    }
}