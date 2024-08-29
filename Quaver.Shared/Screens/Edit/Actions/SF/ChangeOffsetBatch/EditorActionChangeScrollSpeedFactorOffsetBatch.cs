using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.SF.ChangeOffsetBatch
{
    [MoonSharpUserData]
    public class EditorActionChangeScrollSpeedFactorOffsetBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ChangeScrollSpeedFactorOffsetBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<ScrollSpeedFactorInfo> ScrollSpeedFactors { get; }

        private float Offset { get; }

        [MoonSharpVisible(false)]
        public EditorActionChangeScrollSpeedFactorOffsetBatch(EditorActionManager manager, Qua workingMap, List<ScrollSpeedFactorInfo> SFs,
            float offset)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollSpeedFactors = SFs;
            Offset = offset;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            foreach (var tp in ScrollSpeedFactors)
                tp.StartTime += Offset;

            ActionManager.TriggerEvent(Type, new EditorChangedScrollSpeedFactorOffsetBatchEventArgs(ScrollSpeedFactors, Offset));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionChangeScrollSpeedFactorOffsetBatch(ActionManager, WorkingMap, ScrollSpeedFactors, -Offset).Perform();
    }
}
