using System;
using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.Layers.Create;
using Wobble.Bindables;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Remove
{
    [MoonSharpUserData]
    public class EditorActionRemoveLayer : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RemoveLayer;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public EditorLayerInfo Layer { get; }

        /// <summary>
        ///     The list of objects that existed in this layer
        /// </summary>
        public List<HitObjectInfo> HitObjectsInLayer { get; set; }

        public BindableList<HitObjectInfo> SelectedHitObjects { get; }

        public int Index { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="workingMap"></param>
        /// <param name="selectedHitObjects"></param>
        /// <param name="layer"></param>
        [MoonSharpVisible(false)]
        public EditorActionRemoveLayer(EditorActionManager manager, Qua workingMap, BindableList<HitObjectInfo> selectedHitObjects,
            EditorLayerInfo layer)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            Layer = layer;
            SelectedHitObjects = selectedHitObjects;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            Index = WorkingMap.EditorLayers.IndexOf(Layer) + 1;

            HitObjectsInLayer = WorkingMap.HitObjects.FindAll(x => x.EditorLayer == Index);

            // Remove the objects from being selected
            HitObjectsInLayer.ForEach(x => SelectedHitObjects.Remove(x));

            WorkingMap.EditorLayers.Remove(Layer);

            new EditorActionRemoveHitObjectBatch(ActionManager, WorkingMap, HitObjectsInLayer).Perform();

            // Find HitObjects at the indices above it and update them
            var hitObjects = WorkingMap.HitObjects.FindAll(x => x.EditorLayer > Index);
            hitObjects.ForEach(x => x.EditorLayer--);

            ActionManager.TriggerEvent(EditorActionType.RemoveLayer, new EditorLayerRemovedEventArgs(Layer));
        }

        [MoonSharpVisible(false)]
        public void Undo()
        {
            new EditorActionPlaceHitObjectBatch(ActionManager, WorkingMap, HitObjectsInLayer).Perform();
            new EditorActionCreateLayer(WorkingMap, ActionManager, SelectedHitObjects, Layer, Index).Perform();
            HitObjectsInLayer.ForEach(x => x.EditorLayer = Index);
        }
    }
}
