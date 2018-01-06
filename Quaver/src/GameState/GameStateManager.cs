﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.GameState.States;
using Quaver.Logging;

namespace Quaver.GameState
{
    internal class GameStateManager
    {
        /// <summary>
        ///     Holds a stack of all the current game states
        /// </summary>
        private Stack<IGameState> States { get; } = new Stack<IGameState>();

        /// <summary>
        ///     Adds a new screen to the stack
        /// </summary>
        /// <param name="newState"></param>
        public void AddState(IGameState newState)
        {
            try
            {
                // Add the screen to the stack
                States.Push(newState);

                // Initialize the screen
                newState.Initialize();
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug(ex);
            }
        }

        /// <summary>
        ///     Removes the top most screen from the stack.
        /// </summary>
        public void RemoveState()
        {
            if (States.Count == 0)
                return;

            try
            {
                States.Peek().UnloadContent();
                States.Pop();
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug(ex);
            }
        }

        /// <summary>
        ///     Clears all the screens from the stack.
        /// </summary>
        public void ClearStates()
        {
            while (States.Count > 0)
            {
                RemoveState();
            }
        }

        /// <summary>
        ///     Removes all screens from the stack and adds a new one.
        /// </summary>
        /// <param name="screen"></param>
        public void ChangeState(IGameState screen)
        {
            try
            {
                ClearStates();
                AddState(screen);
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug(ex);
            }
        }
    
        /// <summary>
        ///     Updates the top screen.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(double dt)
        {
            try
            {
                if (States.Count == 0)
                    return;

                if (States.Peek().UpdateReady)
                    States.Peek().Update(dt);
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug(ex);
            }
        }

        /// <summary>
        /// Draws the current GameState
        /// </summary>
        public void Draw()
        {
            try
            {
                if (States.Count == 0)
                    return;

                if (States.Peek().UpdateReady)
                    States.Peek().Draw();
            }
            catch (Exception ex)
            {
                Debug(ex);
            }
        }

        private void Debug(Exception ex)
        {
            Logger.Log(ex.TargetSite + "\n" + ex.StackTrace + "\n" + ex.Message + "\n", LogColors.GameError, 5.0f);
        }
    }
}
