using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Layers.Move;
using Quaver.Shared.Screens.Edit.Actions.Layers.Remove;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Merge
{
    public class EditorActionMergeLayers : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.MergeLayers;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private EditorLayerInfo OriginLayer { get; }

        private EditorLayerInfo DestinationLayer { get; }

        private BindableList<HitObjectInfo> SelectedHitObjects { get; }

        private EditorActionMoveObjectsToLayer MoveObjectsToLayerAction { get; }

        private EditorActionRemoveLayer RemoveLayerAction { get; }

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="workingMap"></param>
        /// <param name="originLayer"></param>
        /// <param name="destinationLayer"></param>
        /// <param name="selectedHitObjects"></param>
        public EditorActionMergeLayers(EditorActionManager manager, Qua workingMap, EditorLayerInfo originLayer,
            EditorLayerInfo destinationLayer, BindableList<HitObjectInfo> selectedHitObjects)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            OriginLayer = originLayer;
            DestinationLayer = destinationLayer;
            SelectedHitObjects = selectedHitObjects;

            var HitObjectsInLayer = WorkingMap.HitObjects.FindAll(x => x.EditorLayer == WorkingMap.EditorLayers.FindIndex(l => l == OriginLayer) + 1);

            MoveObjectsToLayerAction = new EditorActionMoveObjectsToLayer(ActionManager, WorkingMap, DestinationLayer, HitObjectsInLayer);
            RemoveLayerAction = new EditorActionRemoveLayer(ActionManager, WorkingMap, SelectedHitObjects, OriginLayer);
        }

        public void Perform()
        {
            MoveObjectsToLayerAction.Perform();
            RemoveLayerAction.Perform();
        }

        public void Undo()
        {
            RemoveLayerAction.Undo();
            MoveObjectsToLayerAction.Undo();
        }
    }
}