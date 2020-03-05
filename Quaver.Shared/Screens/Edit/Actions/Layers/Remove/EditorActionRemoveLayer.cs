using System;
using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.Layers.Create;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Remove
{
    public class EditorActionRemoveLayer : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RemoveLayer;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private EditorLayerInfo Layer { get; }

        /// <summary>
        ///     The list of objects that existed in this layer
        /// </summary>
        private List<HitObjectInfo> HitObjectsInLayer { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="workingMap"></param>
        /// <param name="layer"></param>
        public EditorActionRemoveLayer(EditorActionManager manager, Qua workingMap, EditorLayerInfo layer)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            Layer = layer;
        }

        public void Perform()
        {
            var index = WorkingMap.EditorLayers.IndexOf(Layer) + 1;

            HitObjectsInLayer = WorkingMap.HitObjects.FindAll(x => x.EditorLayer == index);
            WorkingMap.EditorLayers.Remove(Layer);

            new EditorActionRemoveHitObjectBatch(ActionManager, WorkingMap, HitObjectsInLayer).Perform();

            // Find HitObjects at the indices above it and update them
            var hitObjects = WorkingMap.HitObjects.FindAll(x => x.EditorLayer > index);
            hitObjects.ForEach(x => x.EditorLayer--);

            ActionManager.TriggerEvent(EditorActionType.RemoveLayer, new EditorLayerRemovedEventArgs(Layer));
        }

        public void Undo()
        {
            new EditorActionPlaceHitObjectBatch(ActionManager, WorkingMap, HitObjectsInLayer).Perform();
            new EditorActionCreateLayer(WorkingMap, ActionManager, Layer).Perform();
            HitObjectsInLayer.ForEach(x => x.EditorLayer = WorkingMap.EditorLayers.Count);
        }
    }
}