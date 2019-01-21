/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Shared.Screens.Editor.Actions;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using IDrawable = Wobble.Graphics.IDrawable;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets
{
    public abstract class EditorRuleset : IDrawable
    {
        /// <summary>
        /// </summary>
        public EditorScreen Screen { get; }

        /// <summary>
        /// </summary>
        public Qua WorkingMap => Screen.WorkingMap;

        /// <summary>
        ///     The container for the ruleset used to draw things.
        /// </summary>
        public Container Container { get; } = new Container();

        /// <summary>
        ///     Manages all user actions for this editor session.
        /// </summary>
        public EditorActionManager ActionManager { get; protected set; }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorRuleset(EditorScreen screen) => Screen = screen;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            if (DialogManager.Dialogs.Count == 0)
                HandleInput(gameTime);

            Container.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Draw(GameTime gameTime) => Container?.Draw(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public virtual void Destroy() => Container?.Destroy();

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        protected abstract void HandleInput(GameTime gameTime);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected abstract EditorActionManager CreateActionManager();
    }
}