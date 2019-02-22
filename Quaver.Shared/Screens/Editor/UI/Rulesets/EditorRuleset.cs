/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Editor.Actions;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.HitObjects;
using Quaver.Shared.Screens.Editor.UI.Toolkit;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
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
        public List<EditorCompositionToolButton> CompositionToolButtons { get; }

        /// <summary>
        ///     Objects that are currently on the clipboard.
        /// </summary>
        public List<HitObjectInfo> Clipboard { get; } = new List<HitObjectInfo>();

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorRuleset(EditorScreen screen)
        {
            Screen = screen;

            // ReSharper disable once VirtualMemberCallInConstructor
            CompositionToolButtons = CreateCompositionToolButtons();
            AlignCompositionToolButtons();
        }

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

        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected abstract List<EditorCompositionToolButton> CreateCompositionToolButtons();

        /// <summary>
        /// </summary>
        private void AlignCompositionToolButtons()
        {
            var view = (EditorScreenView) Screen.View;

            for (var i = 0; i < CompositionToolButtons.Count; i++)
            {
                var btn = CompositionToolButtons[i];
                btn.Parent = view.CompositionToolbox;

                btn.Y = btn.Height * i + view.CompositionToolbox.HeaderBackground.Height;
            }
        }
    }
}
