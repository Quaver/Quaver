/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Editor.UI.Layering;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal
{
    public class EditorActionEditLayer : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.EditLayer;

        /// <summary>
        /// </summary>
        private EditorLayerCompositor Compositor { get; }

        /// <summary>
        /// </summary>
        private EditorLayerInfo Layer { get; }

        /// <summary>
        /// </summary>
        private string OriginalName { get; }

        /// <summary>
        /// </summary>
        private string OriginalColor { get; }

        /// <summary>
        /// </summary>
        private string NewName { get; }

        /// <summary>
        /// </summary>
        private string NewColor { get; }

        /// <summary>
        /// </summary>
        public EditorActionEditLayer(EditorLayerCompositor compositor, EditorLayerInfo layer, string newName, string newColor)
        {
            Compositor = compositor;
            Layer = layer;
            OriginalColor = Layer.ColorRgb;
            OriginalName = Layer.Name;
            NewName = newName;
            NewColor = newColor;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            Layer.Name = NewName;
            Layer.ColorRgb = NewColor;

            var index = Compositor.ScrollContainer.AvailableItems.IndexOf(Layer);
            Compositor.InvokeUpdatedEvent(EditorLayerUpdateType.Color, Layer, index);

            var drawable = Compositor.ScrollContainer.Pool[index];
            drawable.UpdateContent(Layer, index);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo()
        {
            Layer.Name = OriginalName;
            Layer.ColorRgb = OriginalColor;

            var index = Compositor.ScrollContainer.AvailableItems.IndexOf(Layer);
            Compositor.InvokeUpdatedEvent(EditorLayerUpdateType.Color, Layer, index);

            var drawable = Compositor.ScrollContainer.Pool[index];
            drawable.UpdateContent(Layer, index);
        }
    }
}
