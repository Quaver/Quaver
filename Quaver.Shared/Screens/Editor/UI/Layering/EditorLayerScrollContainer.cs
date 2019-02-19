/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Editor.UI.Layering
{
    public class EditorLayerScrollContainer : PoolableScrollContainer<EditorLayerInfo>
    {
        /// <summary>
        /// </summary>
        public EditorLayerCompositor Compositor { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="compositor"></param>
        /// <param name="poolSize"></param>
        /// <param name="poolStartingIndex"></param>
        /// <param name="size"></param>
        public EditorLayerScrollContainer(EditorLayerCompositor compositor, int poolSize, int poolStartingIndex, ScalableVector2 size)
            : base(CreateLayersList(compositor.Screen.WorkingMap.EditorLayers), poolSize, poolStartingIndex, size, size)
        {
            Compositor = compositor;
            Tint = Color.Transparent;
            Scrollbar.Tint = ColorHelper.HexToColor("#CCCCCC");
            Scrollbar.Width = 3;
            ScrollSpeed = 150;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1500;

            CreatePool();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position) && DialogManager.Dialogs.Count == 0;
            base.Update(gameTime);
        }

        /// <summary>
        ///     Adds a layer to the container. Creates a drawable if necessary
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="index"></param>
        public void AddLayer(EditorLayerInfo layer, int index = -1)
        {
            if (index == -1)
                AddObjectToBottom(layer, true);
        }

        /// <summary>
        ///     Removes an object from the container.
        /// </summary>
        /// <param name="layer"></param>
        public void RemoveLayer(EditorLayerInfo layer) => RemoveObject(layer);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override PoolableSprite<EditorLayerInfo> CreateObject(EditorLayerInfo l, int index) => new EditorDrawableLayer(this, Compositor, l, index);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        protected override void RemoveObject(EditorLayerInfo obj)
        {
            var index = AvailableItems.IndexOf(obj);

            var drawable = (EditorDrawableLayer) Pool.Find(x => x.Index == index);

            AvailableItems.Remove(obj);

            drawable.EditLayerNameButton.Destroy();
            drawable.SelectLayerButton.Destroy();
            drawable.VisibilityCheckbox.Destroy();
            drawable.Destroy();

            RemoveContainedDrawable(drawable);
            Pool.Remove(drawable);

            for (var i = 0; i < Pool.Count; i++)
            {
                Pool[i].Y = (PoolStartingIndex + i) * drawable.HEIGHT;
                Pool[i].Index = i;
            }

            RecalculateContainerHeight();
        }

        /// <summary>
        /// </summary>
        /// <param name="layers"></param>
        /// <returns></returns>
        private static List<EditorLayerInfo> CreateLayersList(IEnumerable<EditorLayerInfo> layers)
        {
            var l = new List<EditorLayerInfo>(layers);
            l.Insert(0, new EditorLayerInfo { Name = "Default Layer", });

            return l;
        }
    }
}
