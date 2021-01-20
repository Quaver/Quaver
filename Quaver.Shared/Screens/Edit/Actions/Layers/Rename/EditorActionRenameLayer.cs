using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Rename
{
    [MoonSharpUserData]
    public class EditorActionRenameLayer : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RenameLayer;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private EditorLayerInfo Layer { get; }

        private string Name { get; }

        private string Previous { get; set; }

        [MoonSharpVisible(false)]
        public EditorActionRenameLayer(EditorActionManager manager, Qua map, EditorLayerInfo layer, string name)
        {
            ActionManager = manager;
            WorkingMap = map;
            Layer = layer;
            Name = name;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            Previous = Layer.Name;
            Layer.Name = Name;

            ActionManager.TriggerEvent(Type, new EditorLayerRenamedEventArgs(Layer, Name));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRenameLayer(ActionManager, WorkingMap, Layer, Previous).Perform();
    }
}
