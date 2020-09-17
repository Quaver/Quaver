using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Visibility
{
    public class EditorActionToggleLayerVisibility : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ToggleLayerVisibility;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private EditorLayerInfo Layer { get; }

        public EditorActionToggleLayerVisibility(EditorActionManager manager, Qua workingMap, EditorLayerInfo layer)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            Layer = layer;
        }

        public void Perform()
        {
            Layer.Hidden = !Layer.Hidden;
        }

        public void Undo() => Perform();
    }
}
