using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SF.RemoveBatch;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.SF.AddBatch
{
    [MoonSharpUserData]
    public class EditorActionAddScrollSpeedFactorBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddScrollSpeedFactorBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<ScrollSpeedFactorInfo> ScrollSpeedFactors { get; }

        [MoonSharpVisible(false)]
        public EditorActionAddScrollSpeedFactorBatch(EditorActionManager manager, Qua workingMap, List<ScrollSpeedFactorInfo> SFs)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollSpeedFactors = SFs;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            ScrollSpeedFactors.ForEach(x => WorkingMap.ScrollSpeedFactors.Add(x));
            WorkingMap.Sort();

            ActionManager.TriggerEvent(Type, new EditorScrollSpeedFactorBatchAddedEventArgs(ScrollSpeedFactors));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveScrollSpeedFactorBatch(ActionManager, WorkingMap, ScrollSpeedFactors).Perform();
    }
}
