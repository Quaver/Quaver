/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Editor.UI.Layering;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal
{
    public class EditorActionAddLayer : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.AddLayer;

        /// <summary>
        /// </summary>
        private EditorLayerCompositor Compositor { get; }

        /// <summary>
        /// </summary>
        private EditorLayerInfo Layer { get; }

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        public EditorActionAddLayer(Qua workingMap, EditorLayerCompositor compositor, EditorLayerInfo l)
        {
            WorkingMap = workingMap;
            Compositor = compositor;
            Layer = l;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            WorkingMap.EditorLayers.Add(Layer);
            Compositor.ScrollContainer.AddLayer(Layer);
            Compositor.SelectedLayerIndex.Value = Compositor.ScrollContainer.AvailableItems.IndexOf(Layer);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => new EditorActionRemoveLayer(WorkingMap, Compositor, Layer).Perform();
    }
}
