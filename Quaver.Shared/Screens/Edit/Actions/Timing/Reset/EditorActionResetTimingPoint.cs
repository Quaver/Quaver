using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeBpm;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffset;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.Reset
{
    [MoonSharpUserData]
    public class EditorActionResetTimingPoint : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ResetTimingPoint;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public TimingPointInfo TimingPoint { get; }

        public float OriginalBpm { get; }

        public float OriginalOffset { get; }

        [MoonSharpVisible(false)]
        public EditorActionResetTimingPoint(EditorActionManager manager, Qua workingMap, TimingPointInfo tp)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingPoint = tp;

            OriginalOffset = tp.StartTime;
            OriginalBpm = tp.Bpm;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            new EditorActionChangeTimingPointBpm(ActionManager, WorkingMap, TimingPoint, 0).Perform();
            new EditorActionChangeTimingPointOffset(ActionManager, WorkingMap, TimingPoint, 0).Perform();
        }

        [MoonSharpVisible(false)]
        public void Undo()
        {
            new EditorActionChangeTimingPointBpm(ActionManager, WorkingMap, TimingPoint, OriginalBpm).Perform();
            new EditorActionChangeTimingPointOffset(ActionManager, WorkingMap, TimingPoint, OriginalOffset).Perform();
        }
    }
}
