using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.GameState
{
    internal class GameStateManager
    {
        /// <summary>
        ///     The GameStateManager instance. This is a singleton class, so there'll only be one.
        /// </summary>   
        private static GameStateManager _instance;
        public static GameStateManager Instance
        {
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-conditional-operator
            get => _instance = _instance ?? new GameStateManager();
        }

        /// <summary>
        ///     The content manager 
        /// </summary>       
        public ContentManager Content { get; set; }

        /// <summary>
        ///     The current graphics manager
        /// </summary>
        public GraphicsDeviceManager Graphics { get; set; }
       
        /// <summary>
        ///     Holds a stack of all the current game states
        /// </summary>
        private readonly Stack<GameStateBase> _states = new Stack<GameStateBase>();

        /// <summary>
        ///     Adds a new screen to the stack
        /// </summary>
        /// <param name="screen"></param>
        public void AddScreen(GameStateBase screen)
        {
            try
            {
                // Add the screen to the stack
                _states.Push(screen);
                // Initialize the screen
                _states.Peek().Initialize();
                // Call the LoadContent on the screen
                if (Content != null)
                {
                    _states.Peek().LoadContent(Content);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
            }
        }

        /// <summary>
        ///     Removes the top most screen from the stack.
        /// </summary>
        public void RemoveScreen()
        {
            if (_states.Count > 0)
            {
                try
                {
                    var screen = _states.Peek();
                    _states.Pop();
                }
                catch (Exception ex)
                {
                    // Log the exception
                }
            }
        }

        /// <summary>
        ///     Clears all the screens from the stack.
        /// </summary>
        public void ClearScreens()
        {
            while (_states.Count > 0)
            {
                _states.Pop();
            }
        }

        /// <summary>
        ///     Removes all screens from the stack and adds a new one.
        /// </summary>
        /// <param name="screen"></param>
        public void ChangeScreen(GameStateBase screen)
        {
            try
            {
                ClearScreens();
                AddScreen(screen);
            }
            catch (Exception ex)
            {
                // Log the exception
            }
        }
    
        /// <summary>
        ///     Updates the top screen.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            try
            {
                if (_states.Count > 0)
                {
                    _states.Peek().Update(gameTime);
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        ///     Renders the top screen.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="windowSize"></param>
        public void Draw(SpriteBatch spriteBatch, Vector2 windowSize)
        {
            try
            {
                if (_states.Count > 0)
                {
                    _states.Peek().Draw(spriteBatch, windowSize);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
            }
        }

        /// <summary>
        ///     Unloads the content from the screen.
        /// </summary>
        public void UnloadContent()
        {
            foreach (var state in _states)
            {
                state.UnloadContent();
            }
        }
    }
}
