using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Logging;

namespace Quaver.src.Gameplay
{
    /// <summary>
    /// This class handles the interaction between note and input.
    /// </summary>
    class NoteManager
    {
        /// <summary>
        /// This method gets called when a key gets pressed.
        /// </summary>
        /// <param name="keyLane"></param>
        internal static void Input(int keyLane, bool keyDown)
        {
            if (keyDown)
            {
                //Do key press stuff
                LogTracker.QuickLog("KeyPress: " + keyLane, Color.Blue);
            }
            else
            {
                //Do key release stuff
                LogTracker.QuickLog("KeyRelease: " + keyLane, Color.DarkBlue);
            }
        }
    }
}
