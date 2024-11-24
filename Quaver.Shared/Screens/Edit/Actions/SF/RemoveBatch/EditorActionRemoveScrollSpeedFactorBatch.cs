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

        public ScrollGroup ScrollGroup { get; }

        [MoonSharpVisible(false)]
        public EditorActionRemoveScrollSpeedFactorBatch(EditorActionManager manager, Qua workingMap, List<ScrollSpeedFactorInfo> sfs, ScrollGroup scrollGroup)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollSpeedFactors = sfs;
            ScrollGroup = scrollGroup ?? manager.EditScreen.SelectedScrollGroup;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            foreach (var sf in ScrollSpeedFactors)
                ScrollGroup.ScrollSpeedFactors.Remove(sf);

            ActionManager.TriggerEvent(Type, new EditorScrollSpeedFactorBatchRemovedEventArgs(ScrollSpeedFactors, ScrollGroup));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionAddScrollSpeedFactorBatch(ActionManager, WorkingMap, ScrollSpeedFactors, ScrollGroup).Perform();
    }
}
