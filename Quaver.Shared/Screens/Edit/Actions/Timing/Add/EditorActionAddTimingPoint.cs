using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Timing.Remove;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.Add
{
    [MoonSharpUserData]
    public class EditorActionAddTimingPoint : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddTimingPoint;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private TimingPointInfo TimingPoint { get; }

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
            WorkingMap.TimingPoints.Add(TimingPoint);
            WorkingMap.Sort();

            ActionManager.TriggerEvent(Type, new EditorTimingPointAddedEventArgs(TimingPoint));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveTimingPoint(ActionManager, WorkingMap, TimingPoint).Perform();
    }
}
