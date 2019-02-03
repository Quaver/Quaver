using System;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Editor.UI.Layering;

namespace Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal
{
    public class EditorActionRemoveLayer : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.RemoveLayer;

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
        /// <param name="workingMap"></param>
        /// <param name="compositor"></param>
        /// <param name="l"></param>
        public EditorActionRemoveLayer(Qua workingMap, EditorLayerCompositor compositor, EditorLayerInfo l)
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
            var index = Compositor.ScrollContainer.AvailableItems.IndexOf(Layer);

            WorkingMap.EditorLayers.Remove(Layer);
            Compositor.ScrollContainer.RemoveLayer(Layer);
            Compositor.SelectedLayerIndex.Value = index - 1;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => new EditorActionAddLayer(WorkingMap, Compositor, Layer).Perform();
    }
}