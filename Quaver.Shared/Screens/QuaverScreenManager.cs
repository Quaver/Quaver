/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Logging;
using Wobble.Screens;

namespace Quaver.Shared.Screens
{
    public static class QuaverScreenManager
    {
        /// <summary>
        ///     The screen that's currently queued to load
        /// </summary>
        public static QuaverScreen QueuedScreen { get; private set; }

        /// <summary>
        ///     If delaying a screen change, this is the amount of time that will have
        ///     to elapse for it to start the fade
        /// </summary>
        private static int DelayedScreenChangeTime { get; set; }

        /// <summary>
        ///     After scheduling a delayed screen change, this keeps track of the amount of
        ///     time that has elapsed. It will take <see cref="DelayedScreenChangeTime"/> amount
        ///     of time for it to begin the fade.
        /// </summary>
        private static double TimeElapsedSinceDelayStarted { get; set; }

        /// <summary>
        ///     Updates the screen manager
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime)
        {
            var game = GameBase.Game as QuaverGame;

            if (QueuedScreen == game?.CurrentScreen || QueuedScreen == null)
                return;

            // Handle delayed screen changes.
            if (DelayedScreenChangeTime != 0)
            {
                TimeElapsedSinceDelayStarted += GameBase.Game.TimeSinceLastFrame;

                if (!(TimeElapsedSinceDelayStarted >= DelayedScreenChangeTime))
                    return;

                Transitioner.FadeIn();
                TimeElapsedSinceDelayStarted = 0;
                DelayedScreenChangeTime = 0;

                return;
            }

            // Wait for fades to complete first.
            if (Transitioner.Blackness.Animations.Count != 0)
                return;

            var oldScreen = game.CurrentScreen;
            ChangeScreen(QueuedScreen);
            oldScreen = null;

            Transitioner.FadeOut();
        }

        /// <summary>
        ///     Changes to a different screen.
        ///     Adds extra functionality such as setting the current screen.
        /// </summary>
        /// <param name="screen"></param>
        public static void ChangeScreen(QuaverScreen screen)
        {
            Logger.Important($"Changed to Screen '{screen.Type}'", LogType.Runtime);

            var game = (QuaverGame) GameBase.Game;
            game.CurrentScreen = screen;

            ScreenManager.ChangeScreen(screen);
            QueuedScreen = screen;

            // Update client status on the server.
            var status = screen.GetClientStatus();

            if (status != null)
                OnlineManager.Client?.UpdateClientStatus(status);

            OtherGameMapDatabaseCache.RunThread();
        }

        /// <summary>
        ///     Schedules the current screen to start changing to the next
        /// </summary>
        /// <param name="newScreen"></param>
        /// <param name="delayFade"></param>
        public static void ScheduleScreenChange(Func<QuaverScreen> newScreen, bool delayFade = false)
        {
            if (!delayFade)
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

                Logger.Important($"Scheduled screen change to: '{QueuedScreen.Type}'. w/ {DelayedScreenChangeTime}ms delay", LogType.Runtime);
            });
        }

        /// <summary>
        ///     Schedule a screen change with a delay before doing so
        /// </summary>
        /// <param name="newScreen"></param>
        /// <param name="delay"></param>
        public static void ScheduleScreenChange(Func<QuaverScreen> newScreen, int delay)
        {
            DelayedScreenChangeTime = delay;
            ScheduleScreenChange(newScreen, true);
        }
    }
}
