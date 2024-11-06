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

        public ScrollGroup ScrollGroup { get; }

        private ScrollSpeedFactorInfo ScrollSpeedFactor { get; }

        [MoonSharpVisible(false)]
        public EditorActionAddScrollSpeedFactor(EditorActionManager manager, Qua workingMap, ScrollSpeedFactorInfo sf, ScrollGroup scrollGroup)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollSpeedFactor = sf;
            ScrollGroup = scrollGroup ?? manager.EditScreen.SelectedScrollGroup;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            ScrollGroup.ScrollSpeedFactors.InsertSorted(ScrollSpeedFactor);

            ActionManager.TriggerEvent(Type, new EditorScrollSpeedFactorAddedEventArgs(ScrollSpeedFactor, ScrollGroup));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveScrollSpeedFactor(ActionManager, WorkingMap, ScrollSpeedFactor, ScrollGroup).Perform();
    }
}
