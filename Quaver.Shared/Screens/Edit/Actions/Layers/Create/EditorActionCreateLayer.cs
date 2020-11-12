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

        private int Index { get; }

        public EditorActionCreateLayer(Qua workingMap, EditorActionManager actionManager, BindableList<HitObjectInfo> selectedHitObjects,
            EditorLayerInfo layer, int index = -1)
        {
            WorkingMap = workingMap;
            ActionManager = actionManager;
            Layer = layer;
            SelectedHitObjects = selectedHitObjects;
            Index = index;
        }

        public void Perform()
        {
            if (!WorkingMap.EditorLayers.Contains(Layer))
            {
                if (Index >= 0)
                    WorkingMap.EditorLayers.Insert(Index, Layer);
                else
                    WorkingMap.EditorLayers.Add(Layer);
            }

            ActionManager.TriggerEvent(EditorActionType.CreateLayer, new EditorLayerCreatedEventArgs(Layer, Index));
        }

        public void Undo() => new EditorActionRemoveLayer(ActionManager, WorkingMap, SelectedHitObjects, Layer).Perform();
    }
}
