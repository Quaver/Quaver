using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SV.Remove;

namespace Quaver.Shared.Screens.Edit.Actions.SV.Add
{
    public class EditorActionAddScrollVelocity : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddScrollVelocity;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private SliderVelocityInfo ScrollVelocity { get; }

        public EditorActionAddScrollVelocity(EditorActionManager manager, Qua workingMap, SliderVelocityInfo sv)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollVelocity = sv;
        }

        public void Perform()
        {
            WorkingMap.SliderVelocities.Add(ScrollVelocity);
            WorkingMap.Sort();

            ActionManager.TriggerEvent(Type, new EditorScrollVelocityAddedEventArgs(ScrollVelocity));
        }

        public void Undo() => new EditorActionRemoveScrollVelocity(ActionManager, WorkingMap, ScrollVelocity).Perform();
    }
}