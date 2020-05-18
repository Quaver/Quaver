using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Timing.Remove;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.Add
{
    public class EditorActionAddTimingPoint : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddTimingPoint;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private TimingPointInfo TimingPoint { get; }

        public EditorActionAddTimingPoint(EditorActionManager manager, Qua workingMap, TimingPointInfo tp)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingPoint = tp;
        }

        public void Perform()
        {
            WorkingMap.TimingPoints.Add(TimingPoint);
            WorkingMap.Sort();

            ActionManager.TriggerEvent(Type, new EditorTimingPointAddedEventArgs(TimingPoint));
        }

        public void Undo() => new EditorActionRemoveTimingPoint(ActionManager, WorkingMap, TimingPoint).Perform();
    }
}