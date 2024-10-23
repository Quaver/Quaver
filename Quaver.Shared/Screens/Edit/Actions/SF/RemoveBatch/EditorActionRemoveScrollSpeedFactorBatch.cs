using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SF.AddBatch;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.SF.RemoveBatch
{
    [MoonSharpUserData]
    public class EditorActionRemoveScrollSpeedFactorBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RemoveScrollSpeedFactorBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<ScrollSpeedFactorInfo> ScrollSpeedFactors { get; }

        [MoonSharpVisible(false)]
        public EditorActionRemoveScrollSpeedFactorBatch(EditorActionManager manager, Qua workingMap, List<ScrollSpeedFactorInfo> SFs)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollSpeedFactors = SFs;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            foreach (var SF in ScrollSpeedFactors)
                WorkingMap.ScrollSpeedFactors.Remove(SF);

            ActionManager.TriggerEvent(Type, new EditorScrollSpeedFactorBatchRemovedEventArgs(ScrollSpeedFactors));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionAddScrollSpeedFactorBatch(ActionManager, WorkingMap, ScrollSpeedFactors).Perform();
    }
}
