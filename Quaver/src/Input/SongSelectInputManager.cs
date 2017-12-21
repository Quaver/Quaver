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
using Quaver.QuaFile;
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

        public bool LeftMouseIsDown { get; set; }
        public bool RightMouseIsDown { get; set; }

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
        }
    }
}
