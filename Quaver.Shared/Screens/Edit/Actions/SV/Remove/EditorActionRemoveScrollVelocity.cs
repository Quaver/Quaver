using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SV.Add;

namespace Quaver.Shared.Screens.Edit.Actions.SV.Remove
{
    public class EditorActionRemoveScrollVelocity : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RemoveScrollVelocity;
        
        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private SliderVelocityInfo ScrollVelocity { get; }
        
        public EditorActionRemoveScrollVelocity(EditorActionManager manager, Qua workingMap, SliderVelocityInfo sv)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollVelocity = sv;
        }
        
        public void Perform()
        {
            WorkingMap.SliderVelocities.Remove(ScrollVelocity);
            ActionManager.TriggerEvent(Type, new EditorScrollVelocityRemovedEventArgs(ScrollVelocity));
        }

        public void Undo() => new EditorActionAddScrollVelocity(ActionManager, WorkingMap, ScrollVelocity).Perform();
    }
}