using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Timing.Remove;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Helpers;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.Add
{
    [MoonSharpUserData]
    public class EditorActionAddTimingPoint : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddTimingPoint;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public TimingPointInfo TimingPoint { get; }

        [MoonSharpVisible(false)]
        public EditorActionAddTimingPoint(EditorActionManager manager, Qua workingMap, TimingPointInfo tp)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingPoint = tp;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            WorkingMap.TimingPoints.InsertSorted(TimingPoint);
            ActionManager.TriggerEvent(Type, new EditorTimingPointAddedEventArgs(TimingPoint));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveTimingPoint(ActionManager, WorkingMap, TimingPoint).Perform();
    }
}
