using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Audio;
using Quaver.GameState;
using Quaver.Config;
using Quaver.Database;
using Quaver.Logging;
using Quaver.Main;
using Quaver.States;

namespace Quaver.Input
{
    internal class SongSelectInputManager : IInputManager
    {
        /// <summary>
        ///     The current State
        /// </summary>
        public State CurrentState { get; set; } = State.Select;

        public event EventHandler LeftMouseClicked;
        public event EventHandler RightMouseClicked;
        public event EventHandler LeftArrowPressed;
        public event EventHandler RightArrowPressed;
        public event EventHandler DownArrowPressed;
        public event EventHandler UpArrowPressed;

        /// <summary>
        ///     Is determined by whether the left mouse is down
        /// </summary>
        public bool LeftMouseIsDown { get; set; }

        /// <summary>
        ///     Is determined by whether the right mouse is down
        /// </summary>
        public bool RightMouseIsDown { get; set; }

        /// <summary>
        ///     Total amount of scrolling in the current frame
        /// </summary>
        public int CurrentScrollAmount { get; set; }

        /// <summary>
        ///     Total amount of scrolling from the previous frame
        /// </summary>
        private int LastScrollAmount { get; set; } = 0;

        public bool LeftArrowIsDown { get; set; }
        public bool RightArrowIsDown { get; set; }
        public bool DownArrowIsDown { get; set; }
        public bool UpArrowIsDown { get; set; }

        public int MouseYPos { get; set; }

        /// <summary>
        ///     Checks if the given input was given
        /// </summary>
        public void CheckInput()
        {
            // Mouse Event Handling
            // Check for LeftMouseDown
            if (GameBase.MouseState.LeftButton == ButtonState.Pressed)
            {
                if (!LeftMouseIsDown)
                {
                    LeftMouseIsDown = true;
                    LeftMouseClicked?.Invoke(this, null);
                }
            }
            else if (LeftMouseIsDown)
                LeftMouseIsDown = false;

            // Check for RightMouseDown
            if (GameBase.MouseState.RightButton == ButtonState.Pressed)
            {
                if (!RightMouseIsDown)
                {
                    RightMouseIsDown = true;
                    RightMouseClicked?.Invoke(this, null);
                }
            }
            else if (RightMouseIsDown)
                RightMouseIsDown = false;

            // Check scroll wheel input
            CurrentScrollAmount = GameBase.MouseState.ScrollWheelValue - LastScrollAmount;
            LastScrollAmount = GameBase.MouseState.ScrollWheelValue;

            // Keyboard Event Handling
            // Up arrow
            if (GameBase.KeyboardState.IsKeyDown(Keys.Up))
            {
                if (!UpArrowIsDown)
                {
                    UpArrowIsDown = true;
                    UpArrowPressed?.Invoke(this, null);
                }
            }
            else if (UpArrowIsDown)
                UpArrowIsDown = false;

            // Down arrow
            if (GameBase.KeyboardState.IsKeyDown(Keys.Down))
            {
                if (!DownArrowIsDown)
                {
                    DownArrowIsDown = true;
                    DownArrowPressed?.Invoke(this, null);
                }
            }
            else if (DownArrowIsDown)
                DownArrowIsDown = false;

            // Left arrow
            if (GameBase.KeyboardState.IsKeyDown(Keys.Left))
            {
                if (!LeftArrowIsDown)
                {
                    LeftArrowIsDown = true;
                    LeftArrowPressed?.Invoke(this, null);
                }
            }
            else if (LeftArrowIsDown)
                LeftArrowIsDown = false;

            // Right arrow
            if (GameBase.KeyboardState.IsKeyDown(Keys.Right))
            {
                if (!RightArrowIsDown)
                {
                    RightArrowIsDown = true;
                    RightArrowPressed?.Invoke(this, null);
                }
            }
            else if (RightArrowIsDown)
                RightArrowIsDown = false;

            MouseYPos = GameBase.MouseState.Position.Y;
        }
    }
}
