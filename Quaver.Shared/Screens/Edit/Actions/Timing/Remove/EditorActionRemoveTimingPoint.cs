using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Timing.Add;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.Remove
{
    public class EditorActionRemoveTimingPoint : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RemoveTimingPoint;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private TimingPointInfo TimingPoint { get; }

        public EditorActionRemoveTimingPoint(EditorActionManager manager, Qua workingMap, TimingPointInfo tp)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingPoint = tp;
        }

        public void Perform()
        {
            WorkingMap.TimingPoints.Remove(TimingPoint);
            ActionManager.TriggerEvent(Type, new EditorTimingPointRemovedEventArgs(TimingPoint));
        }

        public void Undo() => new EditorActionAddTimingPoint(ActionManager, WorkingMap, TimingPoint).Perform();
    }
}