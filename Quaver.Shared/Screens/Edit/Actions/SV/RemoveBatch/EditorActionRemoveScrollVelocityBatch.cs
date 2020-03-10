using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SV.AddBatch;

namespace Quaver.Shared.Screens.Edit.Actions.SV.RemoveBatch
{
    public class EditorActionRemoveScrollVelocityBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RemoveScrollVelocityBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<SliderVelocityInfo> ScrollVelocities { get; }

        public EditorActionRemoveScrollVelocityBatch(EditorActionManager manager, Qua workingMap, List<SliderVelocityInfo> svs)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollVelocities = svs;
        }

        public void Perform()
        {
            foreach (var sv in ScrollVelocities)
                WorkingMap.SliderVelocities.Remove(sv);

            ActionManager.TriggerEvent(Type, new EditorScrollVelocityBatchRemovedEventArgs(ScrollVelocities));
        }

        public void Undo() => new EditorActionAddScrollVelocityBatch(ActionManager, WorkingMap, ScrollVelocities).Perform();
    }
}