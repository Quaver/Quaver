using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SF.RemoveBatch;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Helpers;

namespace Quaver.Shared.Screens.Edit.Actions.SF.AddBatch
{
    [MoonSharpUserData]
    public class EditorActionAddScrollSpeedFactorBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddScrollSpeedFactorBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<ScrollSpeedFactorInfo> ScrollSpeedFactors { get; }

        public ScrollGroup ScrollGroup { get; }

        [MoonSharpVisible(false)]
        public EditorActionAddScrollSpeedFactorBatch(EditorActionManager manager, Qua workingMap, List<ScrollSpeedFactorInfo> SFs, ScrollGroup scrollGroup)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollSpeedFactors = SFs;
            ScrollGroup = scrollGroup ?? manager.EditScreen.SelectedScrollGroup;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            ScrollGroup.ScrollSpeedFactors.InsertSorted(ScrollSpeedFactors);

            ActionManager.TriggerEvent(Type, new EditorScrollSpeedFactorBatchAddedEventArgs(ScrollSpeedFactors, ScrollGroup));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveScrollSpeedFactorBatch(ActionManager, WorkingMap, ScrollSpeedFactors, ScrollGroup).Perform();
    }
}
