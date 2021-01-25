using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Layers.Remove;
using Wobble.Bindables;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Create
{
    [MoonSharpUserData]
    public class EditorActionCreateLayer : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.CreateLayer;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private EditorLayerInfo Layer { get; }

        private BindableList<HitObjectInfo> SelectedHitObjects { get; }

        private int Index { get; }

        [MoonSharpVisible(false)]
        public EditorActionCreateLayer(Qua workingMap, EditorActionManager actionManager, BindableList<HitObjectInfo> selectedHitObjects,
            EditorLayerInfo layer, int index = -1)
        {
            WorkingMap = workingMap;
            ActionManager = actionManager;
            Layer = layer;
            SelectedHitObjects = selectedHitObjects;
            Index = index;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            if (!WorkingMap.EditorLayers.Contains(Layer))
            {
                if (Index >= 1)
                {
                    WorkingMap.EditorLayers.Insert(Index - 1, Layer);
                    var hitObjects = WorkingMap.HitObjects.FindAll(x => x.EditorLayer >= Index);
                    hitObjects.ForEach(x => x.EditorLayer++);
                }
                else
                    WorkingMap.EditorLayers.Add(Layer);
            }

            ActionManager.TriggerEvent(EditorActionType.CreateLayer, new EditorLayerCreatedEventArgs(Layer, Index));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveLayer(ActionManager, WorkingMap, SelectedHitObjects, Layer).Perform();
    }
}
