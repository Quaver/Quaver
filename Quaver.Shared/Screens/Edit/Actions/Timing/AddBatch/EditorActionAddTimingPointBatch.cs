using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Timing.RemoveBatch;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.AddBatch
{
    public class EditorActionAddTimingPointBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddTimingPointBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<TimingPointInfo> TimingPoints { get; }

        public EditorActionAddTimingPointBatch(EditorActionManager manager, Qua workingMap, List<TimingPointInfo> tps)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingPoints = tps;
        }

        public void Perform()
        {
            TimingPoints.ForEach(x => WorkingMap.TimingPoints.Add(x));
            WorkingMap.Sort();

            ActionManager.TriggerEvent(Type, new EditorTimingPointBatchAddedEventArgs(TimingPoints));
        }

        public void Undo() => new EditorActionRemoveTimingPointBatch(ActionManager, WorkingMap, TimingPoints).Perform();
    }
}