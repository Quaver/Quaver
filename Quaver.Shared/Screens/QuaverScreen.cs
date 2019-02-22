/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects;
using Wobble.Graphics.UI.Buttons;
using Wobble.Screens;

namespace Quaver.Shared.Screens
{
    public abstract class QuaverScreen : Screen
    {
        /// <summary>
        ///     Called when the first update is called.
        /// </summary>
        private bool FirstUpdateCalled { get; set; }

        /// <summary>
        ///     The type of screen this is.
        /// </summary>
        public abstract QuaverScreenType Type { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override ScreenView View { get; protected set; }

        /// <summary>
        ///     Dictates if the screen is currently exiting.
        /// </summary>
        public bool Exiting { get; private set; }

        /// <summary>
        ///     Event invoked when the screen is about to exit.
        /// </summary>
        public event EventHandler<ScreenExitingEventArgs> ScreenExiting;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!FirstUpdateCalled)
            {
                OnFirstUpdate();
                FirstUpdateCalled = true;
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     Called the first update call.
        ///     Used for when the screen has already finished initializing
        /// </summary>
        public virtual void OnFirstUpdate() => Button.IsGloballyClickable = true;

        /// <summary>
        ///     Called to begin the exit to a new screen
        /// </summary>
        public virtual void Exit(Func<QuaverScreen> screen, int delay = 0)
        {
            Exiting = true;
            Button.IsGloballyClickable = false;

            ScreenExiting?.Invoke(this, new ScreenExitingEventArgs());

            if (delay > 0)
                QuaverScreenManager.ScheduleScreenChange(screen, delay);
            else
                QuaverScreenManager.ScheduleScreenChange(screen);

            ScreenExiting = null;
        }

        /// <summary>
        ///   Creates a user client status for this screen.
        /// </summary>
        /// <returns></returns>
        public abstract UserClientStatus GetClientStatus();
    }
}
