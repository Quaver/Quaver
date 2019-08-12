/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Menu.UI.Jukebox;

namespace Quaver.Shared.Screens.Editor.UI.Layering
{
    public class EditorLayerVisiblityCheckbox : JukeboxButton
    {
        /// <summary>
        /// </summary>
        private EditorDrawableLayer DrawableLayer { get; }

        /// <summary>
        /// </summary>
        public EditorLayerVisiblityCheckbox(EditorDrawableLayer drawable) : base(GetVisiblityTexture(drawable.Item.Hidden))
        {
            DrawableLayer = drawable;
            Clicked += (sender, args) => ToggleLayerVisiblity();
        }

        /// <summary>
        /// </summary>
        private void ToggleLayerVisiblity()
        {
            DrawableLayer.Item.Hidden = !DrawableLayer.Item.Hidden;
            Image = GetVisiblityTexture(DrawableLayer.Item.Hidden );
            DrawableLayer.LayerCompositor.InvokeUpdatedEvent(EditorLayerUpdateType.Visibility, DrawableLayer.Item, DrawableLayer.Index);
        }

        /// <summary>
        /// </summary>
        /// <param name="hidden"></param>
        /// <returns></returns>
        private static Texture2D GetVisiblityTexture(bool hidden)
            => hidden ? FontAwesome.Get(FontAwesomeIcon.fa_check_box_empty) : FontAwesome.Get(FontAwesomeIcon.fa_check);
    }
}
