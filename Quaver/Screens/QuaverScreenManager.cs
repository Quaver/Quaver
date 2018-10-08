using System;
using System.Collections.Generic;
using System.Text;
using Wobble.Logging;
using Wobble.Screens;

namespace Quaver.Screens
{
    public static class QuaverScreenManager
    {
        /// <summary>
        ///     Changes to a different screen.
        ///     Adds extra functionality such as setting the current screen.
        /// </summary>
        /// <param name="screen"></param>
        public static void ChangeScreen(QuaverScreen screen)
        {
            Logger.Debug($"Changed to Screen '{screen.Type}'", LogType.Runtime);
            ScreenManager.ChangeScreen(screen);
        }
    }
}
