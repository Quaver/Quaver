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
using Quaver.GameState.States;
using Quaver.Logging;
using Quaver.Replays;
using Quaver.GameState.Gameplay;

namespace Quaver.Input
{
    internal class SongSelectInputManager : IInputManager
    {
        /// <summary>
        ///     The current State
        /// </summary>
        public State CurrentState { get; set; } = State.SongSelect;

        public event EventHandler LeftMouseDown;
        public event EventHandler LeftMouseUp;
        public event EventHandler RightMouseDown;
        public event EventHandler RightMouseUp;

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
                    LeftMouseDown?.Invoke(this, null);
                }
            }
            // Check for LeftMouseUp
            else
            {
                if (LeftMouseIsDown)
                {
                    LeftMouseIsDown = false;
                    LeftMouseUp?.Invoke(this, null);
                }
            }

            // Check for RightMouseDown
            if (GameBase.MouseState.RightButton == ButtonState.Pressed)
            {
                if (!RightMouseIsDown)
                {
                    RightMouseIsDown = true;
                    RightMouseDown?.Invoke(this, null);
                }
            }
            // Check for RightMouseUp
            else
            {
                if (RightMouseIsDown)
                {
                    RightMouseIsDown = false;
                    RightMouseUp?.Invoke(this, null);
                }
            }

            // Check scroll wheel input
            CurrentScrollAmount = GameBase.MouseState.ScrollWheelValue - LastScrollAmount;
            LastScrollAmount = GameBase.MouseState.ScrollWheelValue;
        }
    }
}
