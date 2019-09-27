/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Screens;
using Wobble.Window;

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
        public bool Exiting { get; set; }

        /// <summary>
        ///     Event invoked when the screen is about to exit.
        /// </summary>
        public event EventHandler<ScreenExitingEventArgs> ScreenExiting;

        /// <summary>
        ///     The currently active right click options for the screen
        /// </summary>
        private RightClickOptions ActiveRightClickOptions { get; set; }

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
        public virtual void Exit(Func<QuaverScreen> screen, int delay = 0, QuaverScreenChangeType type = QuaverScreenChangeType.CompleteChange)
        {
            Exiting = true;
            Button.IsGloballyClickable = false;
            ActiveRightClickOptions?.Close();

            ScreenExiting?.Invoke(this, new ScreenExitingEventArgs());

            if (delay > 0)
                QuaverScreenManager.ScheduleScreenChange(screen, delay, type);
            else
                QuaverScreenManager.ScheduleScreenChange(screen, false, type);

            ScreenExiting = null;
        }

        /// <summary>
        ///     Activates right click options for the screen
        /// </summary>
        /// <param name="rco"></param>
        public void ActivateRightClickOptions(RightClickOptions rco)
        {
            if (ActiveRightClickOptions != null)
            {
                ActiveRightClickOptions.Visible = false;
                ActiveRightClickOptions.Parent = null;
                ActiveRightClickOptions.Destroy();
            }

            ActiveRightClickOptions = rco;
            ActiveRightClickOptions.Parent = View.Container;

            ActiveRightClickOptions.ItemContainer.Height = 0;
            ActiveRightClickOptions.Visible = true;

            var x = MathHelper.Clamp(MouseManager.CurrentState.X - ActiveRightClickOptions.Width, 0,
                WindowManager.Width - ActiveRightClickOptions.Width);

            var y = MathHelper.Clamp(MouseManager.CurrentState.Y, 0,
                WindowManager.Height - ActiveRightClickOptions.Items.Count * ActiveRightClickOptions.Items.First().Height - 60);

            ActiveRightClickOptions.Position = new ScalableVector2(x, y);

            ActiveRightClickOptions.Open(350);
        }

        /// <summary>
        ///     Exits and removes the top screen
        /// </summary>
        /// <param name="screen"></param>
        public void RemoveTopScreen(QuaverScreen screen) => Exit(() => screen, 0, QuaverScreenChangeType.RemoveTopScreen);

        /// <summary>
        ///   Creates a user client status for this screen.
        /// </summary>
        /// <returns></returns>
        public abstract UserClientStatus GetClientStatus();
    }
}
