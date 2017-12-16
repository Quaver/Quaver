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

        public event EventHandler MouseDown;
        public event EventHandler MouseUp;

        public bool MouseIsDown { get; set; }

        /// <summary>
        ///     Checks if the given input was given
        /// </summary>
        public void CheckInput(Qua qua, bool skippable, List<ReplayFrame> ReplayFrames)
        {
            // Mouse Event Handling
            // Check for MouseDown
            if (GameBase.MouseState.LeftButton == ButtonState.Pressed)
            {
                if (!MouseIsDown)
                {
                    MouseIsDown = true;
                    MouseDown?.Invoke(this, null);
                }
            }
            // Check for MouseUp
            else
            {
                if (MouseIsDown)
                {
                    MouseIsDown = false;
                    MouseUp?.Invoke(this, null);
                }
            }
        }
    }
}
