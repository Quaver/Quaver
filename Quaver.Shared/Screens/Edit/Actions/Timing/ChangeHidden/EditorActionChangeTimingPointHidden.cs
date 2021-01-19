using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.ChangeHidden
{
    public class EditorActionChangeTimingPointHidden : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ChangeTimingPointHidden;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private TimingPointInfo TimingPoint { get; }

        private bool OriginalHidden { get; }

        private bool NewHidden { get; }

        public EditorActionChangeTimingPointHidden(EditorActionManager manager, Qua workingMap, TimingPointInfo tp, bool newHidden)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingPoint = tp;

            OriginalHidden = tp.Hidden;
            NewHidden = newHidden;
        }

        public void Perform()
        {
            TimingPoint.Hidden = NewHidden;
            ActionManager.TriggerEvent(Type, new EditorTimingPointHiddenChangedEventArgs(OriginalHidden, NewHidden));
        }

        public void Undo() => new EditorActionChangeTimingPointHidden(ActionManager, WorkingMap, TimingPoint, OriginalHidden).Perform();
    }
}