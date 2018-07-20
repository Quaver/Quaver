using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Quaver.States
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
            // Add the screen to the stack
            States.Push(newState);

            // Initialize the screen
            if (!newState.UpdateReady)
                newState.Initialize();
        }

        /// <summary>
        ///     Removes the top most screen from the stack.
        /// </summary>
        public void RemoveState()
        {
            if (States.Count == 0)
                return;

            States.Peek().UnloadContent();
            States.Pop();
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
            ClearStates();
            AddState(screen);
        }

        /// <summary>
        ///     Updates the top screen.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            if (States.Count == 0)
                return;

            if (States.Peek().UpdateReady)
                States.Peek().Update(dt);
        }

        /// <summary>
        /// Draws the current GameState
        /// </summary>
        public void Draw()
        {
            if (States.Count == 0)
                return;

            if (States.Peek().UpdateReady)
                States.Peek().Draw();
        }

        /// <summary>
        ///     Asynchronously load a screen and call its initialize method.
        ///     After doing so, the callback action will be called
        /// </summary>
        public static void LoadAsync(Func<IGameState> loadAction, Action callback) => Task.Run(loadAction).ContinueWith(t =>
        {
            t.Result.Initialize();
            t.Result.UpdateReady = true;
            callback();
        });
    }
}
