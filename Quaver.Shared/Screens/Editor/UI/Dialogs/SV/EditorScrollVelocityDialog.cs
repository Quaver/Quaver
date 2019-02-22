/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.SV
{
    public class EditorScrollVelocityDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        private bool IsClosing { get; set; }

        /// <summary>
        /// </summary>
        private EditorScrollVelocityChanger Changer { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorScrollVelocityDialog(EditorScrollVelocityChanger changer) : base(0)
        {
            Changer = changer;
            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 0.75f, 100));

            // ReSharper disable once VirtualMemberCallInConstructor
            CreateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void CreateContent()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Changer.Update(gameTime);
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (Changer.Shown)
                Changer.Draw(gameTime);

            GameBase.Game.SpriteBatch.End();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Changer.Hide();
        }

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        public void Close()
        {
            if (IsClosing)
                return;

            IsClosing = true;
            Animations.Clear();
            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Alpha, 0f, 200));
            ThreadScheduler.RunAfter(() =>
            {
                lock (DialogManager.Dialogs)
                    DialogManager.Dismiss(this);

                ButtonManager.Remove(this);
                IsClosing = false;

                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;
                var view = screen?.View as EditorScreenView;
                view.ControlBar.Parent = view.Container;
            }, 450);
        }
    }
}
