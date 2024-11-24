using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffset
{
    [MoonSharpUserData]
    public class EditorActionChangeTimingPointOffset : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ChangeTimingPointOffset;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public TimingPointInfo TimingPoint { get; }

        public float OriginalOffset { get; }

        public float NewOffset { get; }

        [MoonSharpVisible(false)]
        public EditorActionChangeTimingPointOffset(EditorActionManager manager, Qua workingMap, TimingPointInfo tp, float newOffset)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingPoint = tp;

            OriginalOffset = TimingPoint.StartTime;
            NewOffset = newOffset;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            TimingPoint.StartTime = NewOffset;
            WorkingMap.SortTimingPoints();
            ActionManager.TriggerEvent(Type, new  EditorTimingPointOffsetChangedEventArgs(OriginalOffset, NewOffset));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionChangeTimingPointOffset(ActionManager, WorkingMap, TimingPoint, OriginalOffset).Perform();
    }
}
