using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Create
{
    public class EditorActionCreateLayer : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.CreateLayer;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private EditorLayerInfo Layer { get; }

        public EditorActionCreateLayer(Qua workingMap, EditorActionManager actionManager, EditorLayerInfo layer)
        {
            WorkingMap = workingMap;
            ActionManager = actionManager;
            Layer = layer;
        }

        public void Perform()
        {
            if (!WorkingMap.EditorLayers.Contains(Layer))
                WorkingMap.EditorLayers.Add(Layer);

            ActionManager.TriggerEvent(EditorActionType.CreateLayer, new EditorLayerCreatedEventArgs(Layer));
        }

        public void Undo()
        {
        }
    }
}