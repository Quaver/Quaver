using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SV.AddBatch;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.SV.RemoveBatch
{
    [MoonSharpUserData]
    public class EditorActionRemoveScrollVelocityBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RemoveScrollVelocityBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<SliderVelocityInfo> ScrollVelocities { get; }

        [MoonSharpVisible(false)]
        public EditorActionRemoveScrollVelocityBatch(EditorActionManager manager, Qua workingMap, List<SliderVelocityInfo> svs)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollVelocities = svs;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            foreach (var sv in ScrollVelocities)
                WorkingMap.SliderVelocities.Remove(sv);

            ActionManager.TriggerEvent(Type, new EditorScrollVelocityBatchRemovedEventArgs(ScrollVelocities));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionAddScrollVelocityBatch(ActionManager, WorkingMap, ScrollVelocities).Perform();
    }
}
