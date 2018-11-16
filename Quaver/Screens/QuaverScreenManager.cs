using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Quaver.Graphics.Transitions;
using Quaver.Online;
using Quaver.Scheduling;
using Wobble;
using Wobble.Logging;
using Wobble.Screens;

namespace Quaver.Screens
{
    public static class QuaverScreenManager
    {
        /// <summary>
        ///     The screen that's currently queued to load
        /// </summary>
        public static QuaverScreen QueuedScreen { get; private set; }

        /// <summary>
        ///     Updates the screen manager
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime)
        {
            if (QueuedScreen == null)
                return;

            // Wait for fades to complete first.
            if (Transitioner.Blackness.Animations.Count != 0)
                return;

            ScreenManager.ChangeScreen(QueuedScreen);
            QueuedScreen = null;
            Transitioner.FadeOut();
        }

        /// <summary>
        ///     Changes to a different screen.
        ///     Adds extra functionality such as setting the current screen.
        /// </summary>
        /// <param name="screen"></param>
        public static void ChangeScreen(QuaverScreen screen)
        {
            Logger.Debug($"Changed to Screen '{screen.Type}'", LogType.Runtime);

            var game = (QuaverGame) GameBase.Game;
            game.CurrentScreen = screen;

            ScreenManager.ChangeScreen(screen);

            // Update client status on the server.
            var status = screen.GetClientStatus();
            if (status != null)
                OnlineManager.Client?.UpdateClientStatus(status);
        }

        /// <summary>
        ///     Schedules the current screen to start changing to the next
        /// </summary>
        /// <param name="newScreen"></param>
        public static void ScheduleScreenChange(Func<QuaverScreen> newScreen)
        {
            if (QueuedScreen != null)
            {
                lock (QueuedScreen)
                    QueuedScreen = null;
            }


            Transitioner.FadeIn();

            ThreadScheduler.Run(() =>
            {
                if (QueuedScreen != null)
                {
                    lock (QueuedScreen)
                        QueuedScreen = newScreen();
                }
                else
                {
                    QueuedScreen = newScreen();
                }
            });
        }
    }
}
