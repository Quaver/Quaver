using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SV.RemoveBatch;

namespace Quaver.Shared.Screens.Edit.Actions.SV.AddBatch
{
    public class EditorActionAddScrollVelocityBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddScrollVelocityBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<SliderVelocityInfo> ScrollVelocities { get; }

        public EditorActionAddScrollVelocityBatch(EditorActionManager manager, Qua workingMap, List<SliderVelocityInfo> svs)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollVelocities = svs;
        }

        public void Perform()
        {
            ScrollVelocities.ForEach(x => WorkingMap.SliderVelocities.Add(x));
            WorkingMap.Sort();

            ActionManager.TriggerEvent(Type, new EditorScrollVelocityBatchAddedEventArgs(ScrollVelocities));
        }

        public void Undo() => new EditorActionRemoveScrollVelocityBatch(ActionManager, WorkingMap, ScrollVelocities).Perform();
    }
}