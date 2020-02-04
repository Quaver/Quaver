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
using Wobble.Graphics.UI.Buttons;
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
        ///     The type of screen change to perform
        /// </summary>
        public static QuaverScreenChangeType ChangeType { get; private set; }

        /// <summary>
        ///     The previous screen that the user was on.
        /// </summary>
        public static QuaverScreenType LastScreen { get; private set; } = QuaverScreenType.None;

        /// <summary>
        ///     If delaying a screen change, this is the amount of time that will have
        ///     to elapse for it to start the fade
        /// </summary>
        public static int DelayedScreenChangeTime { get; private set; }

        /// <summary>
        ///     After scheduling a delayed screen change, this keeps track of the amount of
        ///     time that has elapsed. It will take <see cref="DelayedScreenChangeTime"/> amount
        ///     of time for it to begin the fade.
        /// </summary>
        public static double TimeElapsedSinceDelayStarted { get; private set; }

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

            switch (ChangeType)
            {
                case QuaverScreenChangeType.CompleteChange:
                    ChangeScreen(QueuedScreen);
                    Button.IsGloballyClickable = true;
                    break;
                case QuaverScreenChangeType.AddToStack:
                    AddScreen(QueuedScreen);
                    break;
                case QuaverScreenChangeType.RemoveTopScreen:
                    RemoveTopScreen();

                    Button.IsGloballyClickable = true;
                    var screen = (QuaverScreen) ScreenManager.Screens.Peek();
                    screen.Exiting = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

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

            if (game.CurrentScreen != null)
                LastScreen = game.CurrentScreen.Type;

            game.CurrentScreen = screen;

            ScreenManager.ChangeScreen(screen);
            QueuedScreen = screen;

            // Update client status on the server.
            var status = screen.GetClientStatus();

            if (status != null)
                OnlineManager.Client?.UpdateClientStatus(status);

            OtherGameMapDatabaseCache.RunThread();
            GC.Collect();
        }

        /// <summary>
        ///     Adds a screen to the stack (only adds and doesn't destroy ones under it)
        /// </summary>
        /// <param name="screen"></param>
        public static void AddScreen(QuaverScreen screen)
        {
            Logger.Important($"Changed to Screen '{screen.Type}'", LogType.Runtime);

            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen != null)
                LastScreen = game.CurrentScreen.Type;

            game.CurrentScreen = screen;

            ScreenManager.AddScreen(screen);
            QueuedScreen = screen;

            // Update client status on the server.
            var status = screen.GetClientStatus();

            if (status != null)
                OnlineManager.Client?.UpdateClientStatus(status);

            OtherGameMapDatabaseCache.RunThread();
        }

        /// <summary>
        ///     Removes the top screen from the stack
        /// </summary>
        public static void RemoveTopScreen()
        {
            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen != null)
                LastScreen = game.CurrentScreen.Type;

            ScreenManager.RemoveScreen();

            var screen = (QuaverScreen) ScreenManager.Screens.Peek();
            game.CurrentScreen = screen;
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
        /// <param name="type"></param>
        public static void ScheduleScreenChange(Func<QuaverScreen> newScreen, bool delayFade = false, QuaverScreenChangeType type = QuaverScreenChangeType.CompleteChange)
        {
            ChangeType = type;

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

                Logger.Important($"Scheduled screen change to: '{QueuedScreen.Type}'. w/ {DelayedScreenChangeTime} ms delay", LogType.Runtime);
            });
        }

        /// <summary>
        ///     Schedule a screen change with a delay before doing so
        /// </summary>
        /// <param name="newScreen"></param>
        /// <param name="delay"></param>
        public static void ScheduleScreenChange(Func<QuaverScreen> newScreen, int delay, QuaverScreenChangeType type = QuaverScreenChangeType.CompleteChange)
        {
            DelayedScreenChangeTime = delay;
            ScheduleScreenChange(newScreen, true, type);
        }
    }
}
