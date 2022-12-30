/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Scheduling;
using Wobble.Screens;

namespace Quaver.Shared.Screens
{
    public static class QuaverScreenManager
    {
        /// <summary>
        ///     The previous screen that the user was on.
        /// </summary>
        public static QuaverScreenType LastScreen { get; private set; } = QuaverScreenType.None;

        /// <summary>
        ///     Task that's ran when fetching for leaderboard scores
        /// </summary>
        private static TaskHandler<Func<QuaverScreen>, QuaverScreen> ScreenLoadTask { get; set; }

        public static void Initialize()
        {
            ScreenLoadTask = new TaskHandler<Func<QuaverScreen>, QuaverScreen>(LoadScreen);
            ScreenLoadTask.OnCompleted += OnCompleted;
        }

        /// <summary>
        ///     Schedules a new screen to be loaded.
        /// </summary>
        /// <param name="newScreen"></param>
        /// <param name="switchImmediately"></param>
        /// <param name="delay"></param>
        public static void ScheduleScreenChange(Func<QuaverScreen> newScreen, bool switchImmediately = false, int delay = 0)
        {
            Logger.Important($"Scheduled Screen Change", LogType.Runtime);

            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen != null)
                LastScreen = game.CurrentScreen.Type;

            if (LastScreen == QuaverScreenType.None || switchImmediately)
            {
                ChangeScreen(newScreen());
                return;
            }

            ScreenLoadTask.Run(newScreen, delay);
        }

        /// <summary>
        ///     Loads the new screen in a task.
        /// </summary>
        /// <param name="newScreen"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static QuaverScreen LoadScreen(Func<QuaverScreen> newScreen, CancellationToken token)
        {
            Transitioner.FadeIn();
            var screen = newScreen();

            Logger.Important($"Screen `{screen.Type}` has been loaded proceeding to switch.", LogType.Runtime);
            return screen;
        }

        /// <summary>
        ///     Called when the screen has finished loading.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="screen"></param>
        private static void OnCompleted(object sender, TaskCompleteEventArgs<Func<QuaverScreen>, QuaverScreen> e)
        {
            var game = (QuaverGame) GameBase.Game;

            // Wait for the transitioner to fully fade to black.
            while (Transitioner.Blackness.Animations.Count != 0)
                Thread.Sleep(16);

            // Run this on the next game loop on the main thread.
            game.ScheduledRenderTargetDraws.Add(() => ChangeScreen(e.Result));
        }

        private static void ChangeScreen(QuaverScreen screen)
        {
            var game = (QuaverGame) GameBase.Game;

            ScreenManager.ChangeScreen(screen);
            game.CurrentScreen = screen;

            // Update client status on the server.
            var status = screen.GetClientStatus();

            if (status != null)
                OnlineManager.Client?.UpdateClientStatus(status);

            OtherGameMapDatabaseCache.RunThread();
            GC.Collect();
            Transitioner.FadeOut();
            Logger.Important($"Screen has been switched to type: `{screen.Type}`", LogType.Runtime);
            Button.IsGloballyClickable = true;
        }
    }
}
