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
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Checkboxes;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Menu.Border.Components.Users;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
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
        public RightClickOptions ActiveRightClickOptions { get; private set; }

        /// <summary>
        ///     The currently active tooltip for the screen
        /// </summary>
        public Tooltip ActiveTooltip { get; private set; }

        /// <summary>
        ///     The currently active checkbox container for the screen
        /// </summary>
        private CheckboxContainer ActiveCheckboxContainer { get; set; }

        /// <summary>
        /// </summary>
        public LoggedInUserDropdown ActiveLoggedInUserDropdown { get; private set; }

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

            HandleTooltipAnimation();

            if (DialogManager.Dialogs.Count == 0 &&  MouseManager.IsUniqueClick(MouseButton.Left)
                && ActiveCheckboxContainer != null && ActiveCheckboxContainer.IsOpen && !ActiveCheckboxContainer.IsHovered())
            {
                ActiveCheckboxContainer.Close();
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
                QuaverScreenManager.ScheduleScreenChange(screen, false, delay);
            else
                QuaverScreenManager.ScheduleScreenChange(screen, false, delay);

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
                WindowManager.Height - ActiveRightClickOptions.OpenHeight - 60);

            ActiveRightClickOptions.Position = new ScalableVector2(x, y);

            ActiveRightClickOptions.Open(350);
        }

        /// <summary>
        ///     Activates checkbox container for the screen
        /// </summary>
        /// <param name="container"></param>
        public void ActivateCheckboxContainer(CheckboxContainer container)
        {
            if (ActiveCheckboxContainer != null)
            {
                ActiveCheckboxContainer.Visible = false;
                ActiveCheckboxContainer.Parent = null;
                ActiveCheckboxContainer.Destroy();
            }

            ActiveCheckboxContainer = container;
            ActiveCheckboxContainer.Parent = View.Container;

            ActiveCheckboxContainer.Visible = true;

            var x = MathHelper.Clamp(MouseManager.CurrentState.X - ActiveCheckboxContainer.Width, 0,
                WindowManager.Width - ActiveCheckboxContainer.Width);

            var y = MathHelper.Clamp(MouseManager.CurrentState.Y, 0,
                WindowManager.Height - ActiveCheckboxContainer.Pool.Count * ActiveCheckboxContainer.Pool.First().Height - 60);

            ActiveCheckboxContainer.Position = new ScalableVector2(x, y);
        }

        /// <summary>
        ///     Activates the current tooltip for the screen
        /// </summary>
        /// <param name="tooltip"></param>
        public void ActivateTooltip(Tooltip tooltip)
        {
            if (ActiveTooltip != null)
            {
                ActiveTooltip.Visible = false;
                ActiveTooltip.Parent = null;
            }

            ActiveTooltip = tooltip;
            ActiveTooltip.Parent = View.Container;
            ActiveTooltip.Visible = true;

            var x = MathHelper.Clamp(MouseManager.CurrentState.X - ActiveTooltip.Width, 0,
                WindowManager.Width - ActiveTooltip.Width);

            var y = MathHelper.Clamp(MouseManager.CurrentState.Y, 60, WindowManager.Height - ActiveTooltip.Height - 60);

            ActiveTooltip.Position = new ScalableVector2(x, y);

            ActiveTooltip.Alpha = 0;
            ActiveTooltip.ClearAnimations();
            ActiveTooltip.FadeTo(1, Easing.Linear, 150);
        }

        /// <summary>
        ///     Deactivates the current tooltip for the screen
        /// </summary>
        public void DeactivateTooltip()
        {
            if (ActiveTooltip == null)
                return;

            ActiveTooltip.Visible = false;
            ActiveTooltip.Parent = null;
        }

        /// <summary>
        /// </summary>
        private void HandleTooltipAnimation()
        {
            if (ActiveTooltip == null)
                return;

            ActiveTooltip.X = MathHelper.Clamp(MouseManager.CurrentState.X - ActiveTooltip.Width, 5, WindowManager.Width - 5);
            ActiveTooltip.Y = MathHelper.Clamp(MouseManager.CurrentState.Y - ActiveTooltip.Height - 2, 5, WindowManager.Height - 5);
        }

        /// <summary>
        /// </summary>
        public void ActivateLoggedInUserDropdown(LoggedInUserDropdown dropdown, ScalableVector2 position)
        {
            if (ActiveLoggedInUserDropdown != null)
            {
                ActiveLoggedInUserDropdown.Visible = false;
                ActiveLoggedInUserDropdown.Parent = null;
                ActiveLoggedInUserDropdown.Destroy();
            }

            ActiveLoggedInUserDropdown = dropdown;
            ActiveLoggedInUserDropdown.Parent = View.Container;
            ActiveLoggedInUserDropdown.Visible = true;

            ActiveLoggedInUserDropdown.Position = position;
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
