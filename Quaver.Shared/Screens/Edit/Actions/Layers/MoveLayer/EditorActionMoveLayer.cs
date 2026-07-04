using System.Diagnostics;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.MoveLayer
{
    [MoonSharpUserData]
    public class EditorActionMoveLayer : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.MoveLayer;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public EditorLayerInfo Layer { get; }

        public int TargetIndex { get; }

        public int OriginalIndex { get; }

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="workingMap"></param>
        /// <param name="layer"></param>
        /// <param name="originalIndex">index of the layer in workingMap.EditorLayers</param>
        /// <param name="targetIndex">index of the layer in workingMap.EditorLayers</param>
        [MoonSharpVisible(false)]
        public EditorActionMoveLayer(EditorActionManager manager, Qua workingMap,
            EditorLayerInfo layer, int originalIndex, int targetIndex)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            Layer = layer;
            OriginalIndex = originalIndex;
            TargetIndex = targetIndex;
        }

        private void MoveLayerTo(int fromIndex, int toIndex)
        {
            var layerCount = WorkingMap.EditorLayers.Count;
            Debug.Assert(fromIndex >= 0 && toIndex >= 0);
            Debug.Assert(fromIndex < layerCount && toIndex <= layerCount);
            Debug.Assert(WorkingMap.EditorLayers.IndexOf(Layer) == fromIndex);

            foreach (var hitObject in WorkingMap.HitObjects)
            {
                var originalLayer = hitObject.EditorLayer - 1;
                if (originalLayer == fromIndex)
                {
                    hitObject.EditorLayer = toIndex + 1;
                }
                else if (originalLayer > fromIndex && originalLayer <= toIndex)
                {
                    // Layer between from and to, and the move is forward
                    hitObject.EditorLayer--;
                }
                else if (originalLayer < fromIndex && originalLayer >= toIndex)
                {
                    // Layer between to and from, the move is backwards
                    hitObject.EditorLayer++;
                }
                else
                {
                    // In these cases, the layer remains unchanged.
                    Debug.Assert(originalLayer < fromIndex && originalLayer < toIndex
                                 || originalLayer > fromIndex && originalLayer > toIndex);
                }
            }

            // Move the layer in the list
            WorkingMap.EditorLayers.Remove(Layer);
            WorkingMap.EditorLayers.Insert(toIndex, Layer);
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            MoveLayerTo(OriginalIndex, TargetIndex);
            ActionManager.TriggerEvent(EditorActionType.MoveLayer,
                new EditorLayerMovedEventArgs(Layer, OriginalIndex, TargetIndex));
        }

        [MoonSharpVisible(false)]
        public void Undo()
        {
            MoveLayerTo(TargetIndex, OriginalIndex);
            ActionManager.TriggerEvent(EditorActionType.MoveLayer,
                new EditorLayerMovedEventArgs(Layer, TargetIndex, OriginalIndex));
        }
    }
}