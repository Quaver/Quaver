using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SF.Add;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.SF.Remove
{
    [MoonSharpUserData]
    public class EditorActionRemoveScrollSpeedFactor : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RemoveScrollSpeedFactor;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private ScrollSpeedFactorInfo ScrollSpeedFactor { get; }

        public ScrollGroup ScrollGroup { get; }

        [MoonSharpVisible(false)]
        public EditorActionRemoveScrollSpeedFactor(EditorActionManager manager, Qua workingMap,
            ScrollSpeedFactorInfo sf, ScrollGroup scrollGroup)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollSpeedFactor = sf;
            ScrollGroup = scrollGroup ?? manager.EditScreen.SelectedScrollGroup;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            ScrollGroup.ScrollSpeedFactors.Remove(ScrollSpeedFactor);
            ActionManager.TriggerEvent(Type, new EditorScrollSpeedFactorRemovedEventArgs(ScrollSpeedFactor, ScrollGroup));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionAddScrollSpeedFactor(ActionManager, WorkingMap, ScrollSpeedFactor, ScrollGroup).Perform();
    }
}
