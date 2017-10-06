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
    internal class GameStateManager
    {
        // Instance of the game state manager     
        private static GameStateManager _instance;

        //Managers
        private ContentManager _content;
        private GraphicsDeviceManager _graphics;

        // Stack for the screens     
        private Stack<GameStateBase> _screens = new Stack<GameStateBase>();

        // Sets the content manager
        public void SetContent(ContentManager content)
        {
            _content = content;
        }

        //Creates the GameStateManager Instance
        public static GameStateManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameStateManager();
                }
                return _instance;
            }
        }

        // Adds a new screen to the stack 
        public void AddScreen(GameStateBase screen)
        {
            try
            {
                // Add the screen to the stack
                _screens.Push(screen);
                // Initialize the screen
                _screens.Peek().Initialize();
                // Call the LoadContent on the screen
                if (_content != null)
                {
                    _screens.Peek().LoadContent(_content);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
            }
        }

        // Removes the top screen from the stack
        public void RemoveScreen()
        {
            if (_screens.Count > 0)
            {
                try
                {
                    var screen = _screens.Peek();
                    _screens.Pop();
                }
                catch (Exception ex)
                {
                    // Log the exception
                }
            }
        }

        // Clears all the screen from the list
        public void ClearScreens()
        {
            while (_screens.Count > 0)
            {
                _screens.Pop();
            }
        }

        // Removes all screens from the stack and adds a new one 
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

        // Updates the top screen. 
        public void Update(GameTime gameTime)
        {
            try
            {
                if (_screens.Count > 0)
                {
                    _screens.Peek().Update(gameTime);
                }
            }
            catch (Exception ex)
            {

            }
        }

        // Renders the top screen.
        public void Draw(SpriteBatch spriteBatch, Vector2 WindowSize)
        {
            try
            {
                if (_screens.Count > 0)
                {
                    _screens.Peek().Draw(spriteBatch, WindowSize);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
            }
        }

        // Unloads the content from the screen
        public void UnloadContent()
        {
            foreach (GameStateBase state in _screens)
            {
                state.UnloadContent();
            }

        }
    }
}
