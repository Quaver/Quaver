using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeBpm;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffset;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.Reset
{
    public class EditorActionResetTimingPoint : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ResetTimingPoint;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private TimingPointInfo TimingPoint { get; }

        public float OriginalBpm { get; }

        public float OriginalOffset { get; }

        public EditorActionResetTimingPoint(EditorActionManager manager, Qua workingMap, TimingPointInfo tp)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingPoint = tp;

            OriginalOffset = tp.StartTime;
            OriginalBpm = tp.Bpm;
        }

        public void Perform()
        {
            new EditorActionChangeTimingPointBpm(ActionManager, WorkingMap, TimingPoint, 0).Perform();
            new EditorActionChangeTimingPointOffset(ActionManager, WorkingMap, TimingPoint, 0).Perform();
        }

        public void Undo()
        {
            new EditorActionChangeTimingPointBpm(ActionManager, WorkingMap, TimingPoint, OriginalBpm).Perform();
            new EditorActionChangeTimingPointOffset(ActionManager, WorkingMap, TimingPoint, OriginalOffset).Perform();
        }
    }
}