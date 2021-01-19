using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.ChangeBpm
{
    [MoonSharpUserData]
    public class EditorActionChangeTimingPointBpm : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ChangeTimingPointBpm;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private TimingPointInfo TimingPoint { get; }

        private float OriginalBpm { get; }

        private float NewBpm { get; }

        [MoonSharpVisible(false)]
        public EditorActionChangeTimingPointBpm(EditorActionManager manager, Qua workingMap, TimingPointInfo tp, float newBpm)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingPoint = tp;

            OriginalBpm = tp.Bpm;
            NewBpm = newBpm;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            TimingPoint.Bpm = NewBpm;
            ActionManager.TriggerEvent(Type, new EditorTimingPointBpmChangedEventArgs(OriginalBpm, NewBpm));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionChangeTimingPointBpm(ActionManager, WorkingMap, TimingPoint, OriginalBpm).Perform();
    }
}
