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

        [MoonSharpVisible(false)]
        public EditorActionRemoveScrollSpeedFactor(EditorActionManager manager, Qua workingMap, ScrollSpeedFactorInfo SF)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollSpeedFactor = SF;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            WorkingMap.ScrollSpeedFactors.Remove(ScrollSpeedFactor);
            ActionManager.TriggerEvent(Type, new EditorScrollSpeedFactorRemovedEventArgs(ScrollSpeedFactor));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionAddScrollSpeedFactor(ActionManager, WorkingMap, ScrollSpeedFactor).Perform();
    }
}
