/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Editor.UI.Layering
{
    public class EditorLayerUpdatedEventArgs : EventArgs
    {
        /// <summary>
        ///     The type of update that was performed to this layer.
        /// </summary>
        public EditorLayerUpdateType Type { get; }

        /// <summary>
        ///     The layer that was updated in some way
        /// </summary>
        public EditorLayerInfo Layer { get; }

        /// <summary>
        ///     The index of the layer in the container
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="layer"></param>
        /// <param name="index"></param>
        public EditorLayerUpdatedEventArgs(EditorLayerUpdateType type, EditorLayerInfo layer, int index)
        {
            Type = type;
            Layer = layer;
            Index = index;
        }
    }
}
