using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Layers.Remove;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Create
{
    public class EditorActionCreateLayer : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.CreateLayer;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private EditorLayerInfo Layer { get; }

        private BindableList<HitObjectInfo> SelectedHitObjects { get; }

        public EditorActionCreateLayer(Qua workingMap, EditorActionManager actionManager, BindableList<HitObjectInfo> selectedHitObjects,
            EditorLayerInfo layer)
        {
            WorkingMap = workingMap;
            ActionManager = actionManager;
            Layer = layer;
            SelectedHitObjects = selectedHitObjects;
        }

        public void Perform()
        {
            if (!WorkingMap.EditorLayers.Contains(Layer))
                WorkingMap.EditorLayers.Add(Layer);

            ActionManager.TriggerEvent(EditorActionType.CreateLayer, new EditorLayerCreatedEventArgs(Layer));
        }

        public void Undo() => new EditorActionRemoveLayer(ActionManager, WorkingMap, SelectedHitObjects, Layer).Perform();
    }
}