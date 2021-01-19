using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Visibility
{
    [MoonSharpUserData]
    public class EditorActionToggleLayerVisibility : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ToggleLayerVisibility;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private EditorLayerInfo Layer { get; }

        [MoonSharpVisible(false)]
        public EditorActionToggleLayerVisibility(EditorActionManager manager, Qua workingMap, EditorLayerInfo layer)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            Layer = layer;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            Layer.Hidden = !Layer.Hidden;
        }

        [MoonSharpVisible(false)]
        public void Undo() => Perform();
    }
}
