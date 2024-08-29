using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SF.Remove;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Helpers;

namespace Quaver.Shared.Screens.Edit.Actions.SF.Add
{
    [MoonSharpUserData]
    public class EditorActionAddScrollSpeedFactor : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddScrollSpeedFactor;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private ScrollSpeedFactorInfo ScrollSpeedFactor { get; }

        [MoonSharpVisible(false)]
        public EditorActionAddScrollSpeedFactor(EditorActionManager manager, Qua workingMap, ScrollSpeedFactorInfo SF)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollSpeedFactor = SF;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            WorkingMap.ScrollSpeedFactors.InsertSorted(ScrollSpeedFactor);

            ActionManager.TriggerEvent(Type, new EditorScrollSpeedFactorAddedEventArgs(ScrollSpeedFactor));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveScrollSpeedFactor(ActionManager, WorkingMap, ScrollSpeedFactor).Perform();
    }
}
