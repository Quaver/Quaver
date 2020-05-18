using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Rename
{
    public class EditorActionRenameLayer : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RenameLayer;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private EditorLayerInfo Layer { get; }

        private string Name { get; }

        private string Previous { get; set; }

        public EditorActionRenameLayer(EditorActionManager manager, Qua map, EditorLayerInfo layer, string name)
        {
            ActionManager = manager;
            WorkingMap = map;
            Layer = layer;
            Name = name;
        }

        public void Perform()
        {
            Previous = Layer.Name;
            Layer.Name = Name;

            ActionManager.TriggerEvent(Type, new EditorLayerRenamedEventArgs(Layer, Name));
        }

        public void Undo() => new EditorActionRenameLayer(ActionManager, WorkingMap, Layer, Previous).Perform();
    }
}